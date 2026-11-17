using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEventManagement.Data;
using SmartEventManagement.Models;
namespace SmartEventManagement.Controllers;
public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher = new();
    public AccountController(AppDbContext db) => _db = db;

    [HttpGet] public IActionResult Login(string returnUrl = null) => View(new LoginVm { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm m)
    {
        if (!ModelState.IsValid) return View(m);
        var email = m.Email.Trim().ToLower();
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (u == null || _hasher.VerifyHashedPassword(u, u.PasswordHash, m.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(m);
        }
        await SignIn(u);
        if (!string.IsNullOrEmpty(m.ReturnUrl) && Url.IsLocalUrl(m.ReturnUrl)) return Redirect(m.ReturnUrl);
        return u.Role == "Admin" ? RedirectToAction("Manage", "Events") : RedirectToAction("Index", "Home");
    }

    [HttpGet] public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm m)
    {
        if (!ModelState.IsValid) return View(m);
        var email = m.Email.Trim().ToLower();
        if (await _db.Users.AnyAsync(x => x.Email == email))
        {
            ModelState.AddModelError("Email", "An account with this email already exists.");
            return View(m);
        }
        var u = new AppUser { Name = m.Name.Trim(), Email = email, Role = "User" };
        u.PasswordHash = _hasher.HashPassword(u, m.Password);
        _db.Users.Add(u); await _db.SaveChangesAsync();
        await SignIn(u);
        TempData["Msg"] = "Welcome to EventSphere, " + u.Name + "!";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    private Task SignIn(AppUser u)
    {
        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, u.Id.ToString()), new(ClaimTypes.Name, u.Name),
            new(ClaimTypes.Email, u.Email), new(ClaimTypes.Role, u.Role) };
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }
}
