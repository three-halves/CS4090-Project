using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Db
{
    public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=CS4090.db");
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
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = Guid.Parse("fc771b9e-2a04-42a6-b73a-714d6ddc3feb"),
                Username = "testuser",
                Name = "Test User",
                PasswordHash = AppAuthenticator.GetPasswordHash("Test Password"),
                Organizing = [],
                Attending = [],
                Attendance = []
            });
            modelBuilder.Entity<Event>().HasData(new Event
            {
                Id = Guid.Parse("4ec07ab7-5385-475c-b727-3bf5beda74ed"),
                Title = "Test Event",
                Description = "Test Description",
                DaysOfTheWeek = false,
                Mask = [],
                FinalizedStart = null,
                FinalizedEnd = null,
                OrganizerId = Guid.Parse("fc771b9e-2a04-42a6-b73a-714d6ddc3feb"),
                Attendees = [],
            });
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
        public List<DateTime> Mask { get; set; } = [];
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
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public List<DateTime> Availability { get; set; } = [];
    }

    public enum Privacy
    {
        PRIVATE, PUBLIC_CUMULATIVE, PUBLIC_IDENTIFIED
    }
}
