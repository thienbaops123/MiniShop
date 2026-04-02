using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniShop.Data;
using MiniShop.Helpers;
using MiniShop.Models;

namespace MiniShop.Controllers;

[RequireLogin]
public class OrderController : Controller
{
    private readonly AppDbContext _db;

    public OrderController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = CartSession.GetCart(HttpContext);
        if (!cart.Any()) return RedirectToAction("Index", "Cart");

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Login", "Account");

        bool needsShippingInfo =
            string.IsNullOrWhiteSpace(user.Phone) ||
            string.IsNullOrWhiteSpace(user.Address);

        ViewBag.NeedsShippingInfo = needsShippingInfo;

        // data để hiển thị (và để prefill lần đầu)
        ViewBag.SavedFullName = user.FullName ?? "";
        ViewBag.SavedPhone = user.Phone ?? "";
        ViewBag.SavedAddress = user.Address ?? "";

        ViewBag.Total = cart.Sum(x => x.Price * x.Quantity);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(string shippingFullName, string shippingPhone, string shippingAddress, string paymentMethod)
    {
        Console.WriteLine("PaymentMethod = " + paymentMethod);
        var cart = CartSession.GetCart(HttpContext);
        if (!cart.Any()) return RedirectToAction("Index", "Cart");

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Login", "Account");

        bool needsShippingInfo =
            string.IsNullOrWhiteSpace(user.Phone) ||
            string.IsNullOrWhiteSpace(user.Address);

        // Nếu user CHƯA có phone/address => bắt buộc nhập lần đầu
        if (needsShippingInfo)
        {
            if (string.IsNullOrWhiteSpace(shippingFullName) ||
                string.IsNullOrWhiteSpace(shippingPhone) ||
                string.IsNullOrWhiteSpace(shippingAddress))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin giao hàng.";
                ViewBag.Total = cart.Sum(x => x.Price * x.Quantity);

                ViewBag.NeedsShippingInfo = true;
                ViewBag.SavedFullName = user.FullName ?? "";
                ViewBag.SavedPhone = user.Phone ?? "";
                ViewBag.SavedAddress = user.Address ?? "";

                return View();
            }

            // Lưu vào profile user chỉ khi đang trống (lần đầu)
            if (string.IsNullOrWhiteSpace(user.Phone))
                user.Phone = shippingPhone.Trim();

            if (string.IsNullOrWhiteSpace(user.Address))
                user.Address = shippingAddress.Trim();

            // Nếu bạn muốn cập nhật FullName từ checkout luôn (tuỳ chọn), mở comment:
            // if (string.IsNullOrWhiteSpace(user.FullName)) user.FullName = shippingFullName.Trim();

            await _db.SaveChangesAsync();
        }

        // Dùng dữ liệu đã lưu (ưu tiên user profile)
        string finalFullName =
            !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName!.Trim()
            : (shippingFullName ?? "").Trim();

        string finalPhone =
            !string.IsNullOrWhiteSpace(user.Phone) ? user.Phone!.Trim()
            : (shippingPhone ?? "").Trim();

        string finalAddress =
            !string.IsNullOrWhiteSpace(user.Address) ? user.Address!.Trim()
            : (shippingAddress ?? "").Trim();

        // Nếu vì lý do nào đó vẫn thiếu thì báo lỗi (an toàn)
        if (string.IsNullOrWhiteSpace(finalFullName) ||
            string.IsNullOrWhiteSpace(finalPhone) ||
            string.IsNullOrWhiteSpace(finalAddress))
        {
            ViewBag.Error = "Thiếu thông tin giao hàng. Vui lòng cập nhật ở Profile hoặc nhập tại Checkout.";
            ViewBag.Total = cart.Sum(x => x.Price * x.Quantity);

            ViewBag.NeedsShippingInfo = true;
            ViewBag.SavedFullName = user.FullName ?? "";
            ViewBag.SavedPhone = user.Phone ?? "";
            ViewBag.SavedAddress = user.Address ?? "";

            return View();
        }

        var order = new Order
        {
            UserId = userId,
            CreatedDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            ShippingFullName = finalFullName,
            ShippingPhone = finalPhone,
            ShippingAddress = finalAddress,
            PaymentMethod = paymentMethod,          // 👈 THÊM DÒNG NÀY
            PaymentStatus = "Pending",
        };

        order.Items = cart.Select(x => new OrderItem
        {
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            Price = x.Price
        }).ToList();

        order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        if (paymentMethod == "VNPay")
        {
            return RedirectToAction("CreatePayment", "Payment", new { amount = order.TotalAmount });
        }

        // 👉 COD thì về trang đơn hàng
        return RedirectToAction("Details", new { id = order.Id });
    }

    public async Task<IActionResult> MyOrders()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var orders = await _db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.Id)
            .ToListAsync();

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var role = HttpContext.Session.GetString("Role") ?? "User";

        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        // user chỉ xem order của mình; admin xem tất
        if (role != "Admin" && order.UserId != userId)
            return Forbid();

        return View(order);
    }
}