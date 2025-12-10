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
