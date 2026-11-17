using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SmartEventManagement.Data;
using SmartEventManagement.Models;
namespace SmartEventManagement.Controllers;
public class EventsController : Controller
{
    private readonly AppDbContext _db;
    public EventsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string q, string category)
    {
        var query = _db.Events.Include(e => e.Registrations).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(e => e.Title.Contains(q) || e.Venue.Contains(q));
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(e => e.Category == category);
        ViewBag.Q = q; ViewBag.Category = category;
        ViewBag.Categories = await _db.Events.Select(e => e.Category).Distinct().ToListAsync();
        return View(await query.OrderBy(e => e.Date).ToListAsync());
    }

    [Authorize(Roles = "Admin")] public async Task<IActionResult> Manage() => View(await _db.Events.Include(e => e.Registrations).OrderBy(e => e.Date).ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var e = await _db.Events.Include(x => x.Registrations).FirstOrDefaultAsync(x => x.Id == id);
        return e == null ? NotFound() : View(e);
    }

    [Authorize(Roles = "Admin")] public IActionResult Create() => View(new Event());

    [HttpPost]
    [Authorize(Roles = "Admin")] public async Task<IActionResult> Create(Event e)
    {
        ModelState.Remove(nameof(e.Registrations));
        if (!ModelState.IsValid) return View(e);
        _db.Add(e); await _db.SaveChangesAsync();
        TempData["Msg"] = "Event created."; return RedirectToAction(nameof(Manage));
    }

    [Authorize(Roles = "Admin")] public async Task<IActionResult> Edit(int id)
    {
        var e = await _db.Events.FindAsync(id);
        return e == null ? NotFound() : View(e);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] public async Task<IActionResult> Edit(Event e)
    {
        ModelState.Remove(nameof(e.Registrations));
        if (!ModelState.IsValid) return View(e);
        _db.Update(e); await _db.SaveChangesAsync();
        TempData["Msg"] = "Event updated."; return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Events.Include(x => x.Registrations).FirstOrDefaultAsync(x => x.Id == id);
        if (e != null) { _db.Events.Remove(e); await _db.SaveChangesAsync(); }
        TempData["Msg"] = "Event deleted."; return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [Authorize] public async Task<IActionResult> Register(int id, Registration r)
    {
        var e = await _db.Events.Include(x => x.Registrations).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        ModelState.Remove(nameof(r.Event));
        if (e.Status == "Completed") ModelState.AddModelError("", "This event is over.");
        else if (r.Tickets > e.SeatsLeft) ModelState.AddModelError("", $"Only {e.SeatsLeft} seats left.");
        if (!ModelState.IsValid) { TempData["Err"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(x => x.ErrorMessage)); return RedirectToAction(nameof(Details), new { id }); }
        r.Id = 0; r.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); r.Name = User.Identity.Name; r.Email = User.FindFirstValue(ClaimTypes.Email); r.EventId = id; r.RegisteredOn = DateTime.Now;
        _db.Registrations.Add(r); await _db.SaveChangesAsync();
        TempData["Msg"] = "Registration successful!"; return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    public async Task<IActionResult> MyBookings()
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        return View(await _db.Registrations.Include(r => r.Event).Where(r => r.UserId == uid).OrderByDescending(r => r.RegisteredOn).ToListAsync());
    }

    [HttpPost]
    [Authorize] public async Task<IActionResult> Cancel(int id, int eventId)
    {
        var r = await _db.Registrations.FindAsync(id);
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (r != null && (User.IsInRole("Admin") || r.UserId == uid)) { _db.Remove(r); await _db.SaveChangesAsync(); TempData["Msg"] = "Booking cancelled."; }
        return User.IsInRole("Admin") ? RedirectToAction(nameof(Details), new { id = eventId }) : RedirectToAction(nameof(MyBookings));
    }
}
