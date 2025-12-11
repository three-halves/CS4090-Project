using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static AppAuthenticator;

namespace Db
{
    public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Attendance> Attendance { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=CS4090.db").EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(e => e.Organizing)
                .WithOne(e => e.Organizer)
                .HasForeignKey(e => e.OrganizerId)
                .IsRequired();
            modelBuilder.Entity<User>()
                .HasMany(e => e.Attending)
                .WithMany(e => e.Attendees)
                .UsingEntity<Attendance>();
            modelBuilder.Entity<Attendance>()
                .Property(e => e.Availability)
                .HasConversion(
                    v => v.Select(x => BitConverter.GetBytes(x)).SelectMany(x => x).ToArray(),
                    v => v.Chunk(16).Select(x => BitConverter.ToUInt128(x)).ToList(),
                    new ValueComparer<List<UInt128>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );

            Seed(modelBuilder);
        }

        private void Seed(ModelBuilder modelBuilder)
        {
            // 1. Create 10 Users
            var users = new List<User>();
            // Start at 1 to avoid Guid.Empty (all zeros)
            for (int i = 1; i <= 10; i++)
            {
                users.Add(new User
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-0000000000{i:00}"),
                    Name = $"User {i}",
                    Username = $"user{i}",
                    PasswordHash = AppAuthenticator.GetPasswordHash("password")
                });
            }
            modelBuilder.Entity<User>().HasData(users);

            // 2. Create 3 Events
            var event1Id = Guid.Parse("00000000-0000-0000-0000-000000000100");
            var event2Id = Guid.Parse("00000000-0000-0000-0000-000000000200");
            var event3Id = Guid.Parse("00000000-0000-0000-0000-000000000300");

            var events = new List<Event>
            {
                new Event
                {
                    Id = event1Id,
                    Title = "Weekly Team Sync",
                    Description = "Mandatory sync meeting for the whole team.",
                    DaysOfTheWeek = true,
                    // Mon, Tue, Wed, Thu, Fri (DayNumber 0 to 4 correspond to Jan 1 to Jan 5 year 1)
                    Dates = [new DateOnly(1, 1, 1), new DateOnly(1, 1, 2), new DateOnly(1, 1, 3), new DateOnly(1, 1, 4), new DateOnly(1, 1, 5)], 
                    EarliestTime = 32, // 8:00
                    LatestTime = 72,   // 18:00
                    OrganizerId = users[0].Id,
                    Privacy = Privacy.PUBLIC_IDENTIFIED
                },
                new Event
                {
                    Id = event2Id,
                    Title = "Weekend Hackathon",
                    Description = "A weekend of coding and snacks.",
                    DaysOfTheWeek = false,
                    Dates = [new DateOnly(2025, 6, 7), new DateOnly(2025, 6, 8)],
                    EarliestTime = 36, // 9:00
                    LatestTime = 88,   // 22:00
                    OrganizerId = users[1].Id,
                    Privacy = Privacy.PUBLIC_CUMULATIVE
                },
                new Event
                {
                    Id = event3Id,
                    Title = "Lunch & Learn",
                    Description = "Casual learning session over lunch.",
                    DaysOfTheWeek = true,
                    Dates = [new DateOnly(1, 1, 3), new DateOnly(1, 1, 5)], // Wed, Fri
                    EarliestTime = 44, // 11:00
                    LatestTime = 56,   // 14:00
                    OrganizerId = users[2].Id,
                    Privacy = Privacy.PRIVATE
                }
            };
            modelBuilder.Entity<Event>().HasData(events);

            // 3. Create Attendances (Schedules)
            var attendances = new List<Attendance>();
            var rng = new Random(42); // Fixed seed for reproducibility

            foreach (var evt in events)
            {
                foreach (var user in users)
                {
                    // Randomly decide if user attends (70% chance)
                    if (rng.NextDouble() < 0.7)
                    {
                        var availability = new List<UInt128>();
                        foreach (var date in evt.Dates)
                        {
                            UInt128 mask = 0;
                            // Generate random availability blocks
                            int currentTime = (int)evt.EarliestTime;
                            while (currentTime < evt.LatestTime)
                            {
                                // Determine block length (1 to 4 hours)
                                int blockLen = rng.Next(1, 5) * 4;
                                bool isAvailable = rng.NextDouble() > 0.4; // 60% chance available

                                if (isAvailable)
                                {
                                    for (int k = 0; k < blockLen && (currentTime + k) < evt.LatestTime; k++)
                                    {
                                        mask |= (UInt128.One << (currentTime + k));
                                    }
                                }
                                currentTime += blockLen;
                            }
                            availability.Add(mask);
                        }
                        
                        attendances.Add(new Attendance
                        {
                            EventId = evt.Id,
                            UserId = user.Id,
                            Availability = availability
                        });
                    }
                }
            }
            modelBuilder.Entity<Attendance>().HasData(attendances);
        }
    }

    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        public List<Event> Organizing { get; set; } = [];

        public List<Event> Attending { get; set; } = [];
        public List<Attendance> Attendance { get; set; } = [];
    }

    public class Event
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public bool DaysOfTheWeek { get; set; } = false;
        // If DaysOfTheWeek then Date.DayNumber is 0 for Monday and 6 for Sunday.
        public List<DateOnly> Dates { get; set; } = [];
        // Hour * 4 + Minute / 15
        public uint EarliestTime { get; set; } = 0;
        // Hour * 4 + Minute / 15
        public uint LatestTime { get; set; } = 96;
        public Privacy Privacy { get; set; } = Privacy.PUBLIC_IDENTIFIED;

        public DateTime? FinalizedStart { get; set; } = null;
        public DateTime? FinalizedEnd { get; set; } = null;

        public Guid OrganizerId { get; set; }
        public User? Organizer { get; set; }

        public List<User> Attendees { get; set; } = [];
        public List<Attendance> Attendance { get; set; } = [];
    }

    public class Attendance
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        // List of bitmasks for each date where the least-significant bit is the earliest time.
        public List<UInt128> Availability { get; set; } = [];
    }

    public enum Privacy
    {
        PRIVATE, PUBLIC_CUMULATIVE, PUBLIC_IDENTIFIED
    }
}