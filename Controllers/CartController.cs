using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.Helpers;
using MiniShop.Models;

namespace MiniShop.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _db;

    public CartController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var cart = CartSession.GetCart(HttpContext);
        ViewBag.Total = cart.Sum(x => x.Price * x.Quantity);
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        if (quantity <= 0) quantity = 1;

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return NotFound();

        var cart = CartSession.GetCart(HttpContext);
        var existing = cart.FirstOrDefault(x => x.ProductId == productId);
        if (existing == null)
        {
            cart.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            });
        }
        else
        {
            existing.Quantity += quantity;
        }

        CartSession.SaveCart(HttpContext, cart);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Update(int productId, int quantity)
    {
        var cart = CartSession.GetCart(HttpContext);
        var item = cart.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            item.Quantity = Math.Max(1, quantity);
            CartSession.SaveCart(HttpContext, cart);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Remove(int productId)
    {
        var cart = CartSession.GetCart(HttpContext);
        cart.RemoveAll(x => x.ProductId == productId);
        CartSession.SaveCart(HttpContext, cart);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Clear()
    {
        CartSession.Clear(HttpContext);
        return RedirectToAction("Index");
    }
}