using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.Models;

namespace MiniShop.Controllers;

public class AdminUsersController : Controller
{
    private readonly AppDbContext _db;
    public AdminUsersController(AppDbContext db) => _db = db;

    private bool IsAdmin()
        => HttpContext.Session.GetString("Role") == "Admin";

    public async Task<IActionResult> Index(string? q)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        q = (q ?? "").Trim();

        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qLower = q.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(qLower) ||
                (u.FullName ?? "").ToLower().Contains(qLower));
        }

        var users = await query
            .OrderBy(u => u.Id)
            .ToListAsync();

        ViewBag.Q = q;
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");
        return View(new User { Role = "User", Status = UserStatus.Active });
    }

    [HttpPost]
    public async Task<IActionResult> Create(string email, string fullName, string password, string role, UserStatus status)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        email = (email ?? "").Trim().ToLowerInvariant();
        fullName = (fullName ?? "").Trim();
        role = string.IsNullOrWhiteSpace(role) ? "User" : role.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Thiếu thông tin.";
            return View(new User { Email = email, FullName = fullName, Role = role, Status = status });
        }

        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == email))
        {
            ViewBag.Error = "Email đã tồn tại.";
            return View(new User { Email = email, FullName = fullName, Role = role, Status = status });
        }

        var u = new User
        {
            Email = email,
            FullName = fullName,
            Role = role,
            Status = status,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _db.Users.Add(u);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string fullName, string? phone, string? address, string role,
        UserStatus status, string? banReason, DateTime? bannedUntil)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.FullName = (fullName ?? "").Trim();
        user.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        user.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        user.Role = string.IsNullOrWhiteSpace(role) ? "User" : role.Trim();
        user.Status = status;
        user.BanReason = string.IsNullOrWhiteSpace(banReason) ? null : banReason.Trim();
        user.BannedUntil = bannedUntil;

        await _db.SaveChangesAsync();
        ViewBag.Success = "Đã lưu.";
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(int id, string newPassword)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            TempData["Error"] = "Mật khẩu tối thiểu 6 ký tự.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã reset mật khẩu.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        // không cho tự xoá chính mình (optional)
        var currentId = HttpContext.Session.GetInt32("UserId");
        if (currentId == id)
        {
            TempData["Error"] = "Không thể xoá chính mình.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _db.Users.FindAsync(id);
        if (user == null) return RedirectToAction(nameof(Index));

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đã xoá user.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Ban(int id, string? reason)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var user = await _db.Users.FindAsync(id);
        if (user == null) return RedirectToAction(nameof(Index));

        user.Status = UserStatus.Banned;
        user.BanReason = string.IsNullOrWhiteSpace(reason) ? "Vi phạm chính sách" : reason.Trim();
        user.BannedUntil = null;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Unban(int id)
    {
        if (!IsAdmin()) return RedirectToAction("Login", "Account");

        var user = await _db.Users.FindAsync(id);
        if (user == null) return RedirectToAction(nameof(Index));

        user.Status = UserStatus.Active;
        user.BanReason = null;
        user.BannedUntil = null;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}