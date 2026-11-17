using Microsoft.EntityFrameworkCore;
using SmartEventManagement.Models;
namespace SmartEventManagement.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) { }
    public DbSet<Event> Events { get; set; }
    public DbSet<Registration> Registrations { get; set; }
    public DbSet<AppUser> Users { get; set; }
}
