using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
            modelBuilder.Entity<User>().HasData([new User
            {
                Id = Guid.Parse("fc771b9e-2a04-42a6-b73a-714d6ddc3feb"),
                Username = "Test Organizer",
                Name = "Test Organizer",
                PasswordHash = AppAuthenticator.GetPasswordHash("")
            }, new User {
                Id = Guid.Parse("b97af242-5f11-4880-a4db-24d0f2c9d930"),
                Username = "Test Attendee 1",
                Name = "Test Attendee 1",
                PasswordHash = AppAuthenticator.GetPasswordHash("")
            }, new User {
                Id = Guid.Parse("0caa1846-397c-435b-abce-46509cb6dc48"),
                Username = "Test Attendee 2",
                Name = "Test Attendee 2",
                PasswordHash = AppAuthenticator.GetPasswordHash("")
            }]);
            /* modelBuilder.Entity<Event>().HasData(new Event
            {
                Id = Guid.Parse("4ec07ab7-5385-475c-b727-3bf5beda74ed"),
                Title = "Test Event",
                Description = "Test Description",
                DaysOfTheWeek = false,
                Dates = [new(2025, 12, 20), new(2025, 12, 22), new(2026, 1, 10)],
                EarliestTime = 10,
                LatestTime = 90,
                OrganizerId = Guid.Parse("fc771b9e-2a04-42a6-b73a-714d6ddc3feb")
            }); */
            /* modelBuilder.Entity<Attendance>().HasData([new Attendance {
                UserId = Guid.Parse("b97af242-5f11-4880-a4db-24d0f2c9d930"),
                EventId = Guid.Parse("4ec07ab7-5385-475c-b727-3bf5beda74ed"),
                Availability = [new UInt128(0xb20b026729df2b6du, 0xd5c7155713273cd5u), new UInt128(0xa6cbb129d9fa306fu, 0x6596df69b65ae525u), new UInt128(0xdd03216216389d6cu, 0x4de9abe7903ee833u)]
            }, new Attendance {
                UserId = Guid.Parse("b97af242-5f11-4880-a4db-24d0f2c9d930"),
                EventId = Guid.Parse("4ec07ab7-5385-475c-b727-3bf5beda74ed"),
                Availability = [new UInt128(0x7e4d774ba94f3c3cu ,0x048ffbb51bcac59au), new UInt128(0x3a475a9a3af3858bu, 0xfd4305cb1a131cdu), new UInt128(0x3d4a19a8a29449eeu ,0x6a554d93abb45d14u)]
            }]); */
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
        public User Organizer { get; set; }

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
