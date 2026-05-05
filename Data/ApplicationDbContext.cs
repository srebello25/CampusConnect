using CampusConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusConnect.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Registration> Registrations { get; set; }
    public DbSet<SupportRequest> SupportRequests { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
}