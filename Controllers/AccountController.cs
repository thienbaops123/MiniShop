using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.Models;

namespace MiniShop.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    private static CookieOptions RememberMeCookieOptions()
        => new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            // Secure = true // bật nếu bạn chạy HTTPS
        };

    // Không cho BCrypt.Verify làm crash khi hash trong DB bị sai format
    private static bool SafeVerifyPassword(string? password, string? passwordHash)
    {
        password ??= "";
        passwordHash ??= "";

        try
        {
            // BCrypt hash hợp lệ thường bắt đầu bằng "$2"
            if (!passwordHash.StartsWith("$2"))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        email = (email ?? "").Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        if (user == null || !SafeVerifyPassword(password, user.PasswordHash))
        {
            ViewBag.Error = "Email hoặc mật khẩu không đúng.";
            return View();
        }

        // Session
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Email", user.Email);
        HttpContext.Session.SetString("FullName", user.FullName ?? "");
        HttpContext.Session.SetString("Role", user.Role);

        // Remember-me cookie (chỉ lưu UserId)
        Response.Cookies.Append("MiniShop.UserId", user.Id.ToString(), RememberMeCookieOptions());

        return RedirectToAction("Index", "Product");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string email, string fullName, string password)
    {
        email = (email ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
            return View();
        }

        var exists = await _db.Users.AnyAsync(x => x.Email.ToLower() == email);
        if (exists)
        {
            ViewBag.Error = "Email đã tồn tại.";
            return View();
        }

        var user = new User
        {
            Email = email,
            FullName = fullName.Trim(),
            Role = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // auto login (Session)
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Email", user.Email);
        HttpContext.Session.SetString("FullName", user.FullName ?? "");
        HttpContext.Session.SetString("Role", user.Role);

        // Remember-me cookie (chỉ lưu UserId)
        Response.Cookies.Append("MiniShop.UserId", user.Id.ToString(), RememberMeCookieOptions());

        return RedirectToAction("Index", "Product");
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();

        // xóa remember-me cookie
        Response.Cookies.Delete("MiniShop.UserId");

        await Task.CompletedTask;
        return RedirectToAction("Index", "Product");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null) return RedirectToAction("Login");

        var user = await _db.Users.FindAsync(userId.Value);
        if (user == null) return RedirectToAction("Login");

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Profile(string fullName, string? phone, string? address)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null) return RedirectToAction("Login");

        var user = await _db.Users.FindAsync(userId.Value);
        if (user == null) return RedirectToAction("Login");

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ViewBag.Error = "Họ tên không được để trống.";
            return View(user);
        }

        user.FullName = fullName.Trim();
        user.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        user.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();

        await _db.SaveChangesAsync();

        // update session để hiện fullname ngay trên navbar
        HttpContext.Session.SetString("FullName", user.FullName ?? "");

        ViewBag.Success = "Cập nhật profile thành công.";
        return View(user);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null) return RedirectToAction("Login");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null) return RedirectToAction("Login");

        var user = await _db.Users.FindAsync(userId.Value);
        if (user == null) return RedirectToAction("Login");

        if (!SafeVerifyPassword(currentPassword, user.PasswordHash))
        {
            ViewBag.Error = "Mật khẩu hiện tại không đúng.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            ViewBag.Error = "Mật khẩu mới tối thiểu 6 ký tự.";
            return View();
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();

        ViewBag.Success = "Đổi mật khẩu thành công.";
        return View();
    }
}