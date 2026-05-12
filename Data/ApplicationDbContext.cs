using CampusConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Data;

public class ApplicationDbContext : DbContext
{
    // Creates the database connection
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Student table
    public DbSet<Student> Students { get; set; }

    // Event table
    public DbSet<Event> Events { get; set; }

    // Venue table
    public DbSet<Venue> Venues { get; set; }

    // Admin table
    public DbSet<Admin> Admins { get; set; }

    // Registration table
    public DbSet<Registration> Registrations { get; set; }

    // Support request table
    public DbSet<SupportRequest> SupportRequests { get; set; }

    // Announcement table
    public DbSet<Announcement> Announcements { get; set; }
}