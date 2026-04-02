using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.Models;

namespace MiniShop.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    // /Product?q=...&categoryId=1&sort=price_asc&page=1
    [HttpGet]
    public async Task<IActionResult> Index(string? q, int? categoryId, string? sort, int page = 1)
    {
        const int pageSize = 8;

        q = (q ?? "").Trim();
        sort = string.IsNullOrWhiteSpace(sort) ? "new" : sort;

        if (page < 1) page = 1;

        var query = _db.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q));

        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.Id) // new
        };

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        if (page > totalPages) page = totalPages;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.Query = q;
        ViewBag.CategoryId = categoryId ?? 0;
        ViewBag.Sort = sort;
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();
        var comments = _db.Comments
        .Where(x => x.ProductId == id)
        .Include(x => x.User)
        .OrderByDescending(x => x.Id)
        .ToList();

        ViewBag.Comments = comments;
        return View(product);
    }
    [HttpPost]
    public IActionResult AddComment(int productId, string content)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return RedirectToAction("Login", "Account");

        var comment = new Comment
        {
            ProductId = productId,
            UserId = userId.Value,
            Content = content,
            CreatedAt = DateTime.Now
        };

        _db.Comments.Add(comment);
        _db.SaveChanges();

        return RedirectToAction("Details", new { id = productId });
    }

    // ================== DELETE COMMENT ==================
    [HttpPost]
    public IActionResult DeleteComment(int id, int productId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        var comment = _db.Comments.FirstOrDefault(x => x.Id == id);

        if (comment != null && comment.UserId == userId)
        {
            _db.Comments.Remove(comment);
            _db.SaveChanges();
        }

        return RedirectToAction("Details", new { id = productId });
    }

    // ================== EDIT COMMENT ==================
    [HttpPost]
    public IActionResult EditComment(int id, string content, int productId, int rating)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        var comment = _db.Comments.FirstOrDefault(x => x.Id == id);

        if (comment != null && comment.UserId == userId)
        {
            comment.Content = content;
            comment.Rating = rating; // 👈 THÊM DÒNG NÀY

            _db.SaveChanges();
        }

        return RedirectToAction("Details", new { id = productId });
    }
}