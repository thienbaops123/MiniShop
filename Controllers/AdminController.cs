using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.Helpers;
using MiniShop.Models;

namespace MiniShop.Controllers;

[RequireAdmin]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    private const int PageSizeProducts = 10;
    private const int PageSizeOrders = 10;

    public AdminController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // GET: /Admin/Products?page=1&q=abc&categoryId=2
    public async Task<IActionResult> Products(int page = 1, string? q = null, int? categoryId = null)
    {
        if (page < 1) page = 1;

        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.Name.Contains(q));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * PageSizeProducts)
            .Take(PageSizeProducts)
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

        ViewBag.Page = page;
        ViewBag.PageSize = PageSizeProducts;
        ViewBag.Total = total;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProduct()
    {
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(Product model, IFormFile? imageFile)
    {
        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();

        if (!ModelState.IsValid) return View(model);

        model.ImageUrl = await SaveImageIfAny(imageFile) ?? "https://placehold.co/600x450/6b63d9/ffffff?text=Product";

        _db.Products.Add(model);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> EditProduct(Product model, IFormFile? imageFile)
    {
        var product = await _db.Products.FindAsync(model.Id);
        if (product == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(model);
        }

        product.Name = model.Name;
        product.Price = model.Price;
        product.CategoryId = model.CategoryId;

        var newImage = await SaveImageIfAny(imageFile);
        if (newImage != null)
            product.ImageUrl = newImage;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Products));
    }

    // GET: /Admin/Orders?page=1&q=123&status=Pending
    public async Task<IActionResult> Orders(int page = 1, string? q = null, OrderStatus? status = null)
    {
        if (page < 1) page = 1;

        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();

            if (int.TryParse(q, out var orderId))
                query = query.Where(o => o.Id == orderId);
            else
                query = query.Where(o => (o.User != null && o.User.Email != null && o.User.Email.Contains(q)));
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.Id)
            .Skip((page - 1) * PageSizeOrders)
            .Take(PageSizeOrders)
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.Status = status;

        ViewBag.Page = page;
        ViewBag.PageSize = PageSizeOrders;
        ViewBag.Total = total;

        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(
    int id,
    OrderStatus status,
    int page = 1,
    string? q = null,
    OrderStatus? filterStatus = null)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Orders), new
        {
            page = page,
            q = q,
            status = filterStatus
        });
    }

    public async Task<IActionResult> OrderDetails(int id)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        return View(order);
    }

    private async Task<string?> SaveImageIfAny(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext)) return null;

        var folder = Path.Combine(_env.WebRootPath, "images", "products");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(folder, fileName);

        using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }

        return $"/images/products/{fileName}";
    }
}