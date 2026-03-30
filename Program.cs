using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.Models;

namespace MiniShop;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();

        // Use ONE SQLite DB (minishop.db) in project directory
        var dbPath = Path.Combine(builder.Environment.ContentRootPath, "minishop.db");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.Cookie.Name = ".MiniShop.Session";
            options.IdleTimeout = TimeSpan.FromHours(8);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        var app = builder.Build();

        // SEED USERS (BCrypt) - runs once if not exists
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // ensure DB file/tables exist (works even if you don't use migrations)
            db.Database.EnsureCreated();

            void EnsureUser(string email, string fullName, string password, string role)
            {
                email = (email ?? "").Trim().ToLowerInvariant();

                var exists = db.Users.Any(u => u.Email.ToLower() == email);
                if (exists) return;

                db.Users.Add(new User
                {
                    Email = email,
                    FullName = fullName,
                    Role = role,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
                });

                db.SaveChanges();
            }

            EnsureUser("admin@minishop.local", "MiniShop Admin", "Admin@123", "Admin");
            EnsureUser("user1@minishop.local", "User 1", "User@123", "User");
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        // Nếu bạn đang chạy HTTP để khỏi popup cert,
        // có thể comment dòng này lại để nó không tự redirect qua https.
        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseSession();

        // Auto restore session từ cookie "Remember me"
        app.Use(async (ctx, next) =>
        {
            if (ctx.Session.GetInt32("UserId") is not null)
            {
                await next();
                return;
            }

            if (ctx.Request.Cookies.TryGetValue("MiniShop.UserId", out var uidStr) &&
                int.TryParse(uidStr, out var uid))
            {
                using var scope = ctx.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var user = await db.Users.FirstOrDefaultAsync(x => x.Id == uid);
                if (user is not null)
                {
                    ctx.Session.SetInt32("UserId", user.Id);
                    ctx.Session.SetString("Email", user.Email);
                    ctx.Session.SetString("FullName", user.FullName ?? "");
                    ctx.Session.SetString("Role", user.Role);
                }
                else
                {
                    // cookie rác => xóa
                    ctx.Response.Cookies.Delete("MiniShop.UserId");
                }
            }

            await next();
        });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Product}/{action=Index}/{id?}");

        app.Run();
    }
}