using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEventManagement.Data;
namespace SmartEventManagement.Controllers;
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;
    public async Task<IActionResult> Bookings() => View(await _db.Registrations.Include(r => r.Event).OrderByDescending(r => r.RegisteredOn).ToListAsync());
    public async Task<IActionResult> Users() => View(await _db.Users.OrderBy(u => u.Role).ThenBy(u => u.Name).ToListAsync());
    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u != null && u.Role != "Admin") { _db.Users.Remove(u); await _db.SaveChangesAsync(); TempData["Msg"] = "User removed."; }
        return RedirectToAction(nameof(Users));
    }
}
