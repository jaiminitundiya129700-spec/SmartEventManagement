using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartEventManagement.Data;
using SmartEventManagement.Models;
var b = WebApplication.CreateBuilder(args);
b.Services.AddControllersWithViews();
b.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/AccessDenied"; o.ExpireTimeSpan = TimeSpan.FromHours(8); });
b.Services.AddAuthorization();
b.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=events.db"));
var app = b.Build();
using (var s = app.Services.CreateScope())
{
    var db = s.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Users.Any())
    {
        var h = new PasswordHasher<AppUser>();
        var a = new AppUser { Name = "Admin", Email = "admin@eventsphere.com", Role = "Admin" }; a.PasswordHash = h.HashPassword(a, "Admin@123");
        var u = new AppUser { Name = "Demo Student", Email = "student@eventsphere.com", Role = "User" }; u.PasswordHash = h.HashPassword(u, "Student@123");
        db.Users.AddRange(a, u); db.SaveChanges();
    }
    if (!db.Events.Any())
    {
        db.Events.AddRange(
            new Event { Title = "Tech Fest 2026", Category = "Technical", Venue = "Marwadi University Auditorium", Date = DateTime.Today.AddDays(10), Capacity = 200, Price = 100, Description = "Coding contests, project expo and guest talks." },
            new Event { Title = "Cultural Night", Category = "Cultural", Venue = "Open Air Theatre", Date = DateTime.Today.AddDays(20), Capacity = 500, Price = 50, Description = "Dance, music and drama performances." },
            new Event { Title = "Startup Workshop", Category = "Workshop", Venue = "Seminar Hall 2", Date = DateTime.Today.AddDays(5), Capacity = 60, Price = 0, Description = "Learn how to turn an idea into a startup." },
            new Event { Title = "24-Hour Hackathon", Category = "Technical", Venue = "Computer Lab Block B", Date = DateTime.Today.AddDays(14), Capacity = 120, Price = 200, Description = "Build a working product in 24 hours and win prizes." },
            new Event { Title = "Inter-College Sports Meet", Category = "Sports", Venue = "University Ground", Date = DateTime.Today.AddDays(30), Capacity = 300, Price = 0, Description = "Cricket, football and athletics between colleges." },
            new Event { Title = "Live Music Concert", Category = "Cultural", Venue = "Open Air Theatre", Date = DateTime.Today.AddDays(45), Capacity = 800, Price = 350, Description = "An evening of live bands and DJ night." });
        db.SaveChanges();
    }
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
