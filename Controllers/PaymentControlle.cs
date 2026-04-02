using Microsoft.AspNetCore.Mvc;
using MiniShop.Data;
using MiniShop.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MiniShop.Controllers;

public class PaymentController : Controller
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public PaymentController(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;
    }

    public IActionResult CreatePayment(decimal amount)
    {
        var vnpUrl = _config["VnPay:BaseUrl"];
        var tmnCode = _config["VnPay:TmnCode"];
        var hashSecret = _config["VnPay:HashSecret"];
        //return Content($"URL={vnpUrl} | CODE={tmnCode} | SECRET={hashSecret}");

        //// 👉 tạo mã đơn hàng (txnRef)
        //var txnRef = DateTime.Now.Ticks.ToString();

        var vnpayData = new SortedList<string, string>
        {
             { "vnp_Version", "2.1.0" },
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", tmnCode },
        { "vnp_Amount", ((int)(amount * 100)).ToString() },
        { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
        { "vnp_CurrCode", "VND" },
        { "vnp_IpAddr", "127.0.0.1" },
        { "vnp_Locale", "vn" },
        { "vnp_OrderInfo", "Thanh toan don hang" },
        { "vnp_OrderType", "other" },
        { "vnp_ReturnUrl", _config["VnPay:ReturnUrl"] },
        { "vnp_TxnRef", DateTime.Now.Ticks.ToString() }
        };

        string query = string.Join("&", vnpayData.Select(x => $"{x.Key}={x.Value}"));
        string secureHash = HmacSHA512(hashSecret, query);

        string paymentUrl = vnpUrl + "?" + query + "&vnp_SecureHash=" + secureHash;

        return Redirect(paymentUrl);
    }

    public IActionResult Callback()
    {
        var responseCode = Request.Query["vnp_ResponseCode"];
        var txnRef = Request.Query["vnp_TxnRef"];

        if (responseCode == "00")
        {
            // 👉 lấy đơn hàng gần nhất (tạm thời)
            var order = _context.Orders
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            if (order != null)
            {
                order.PaymentStatus = "Paid";
                order.PaymentMethod = "VNPay";
                order.Status = OrderStatus.Paid; // hoặc "Completed"
                _context.SaveChanges();
            }

            return Content("Thanh toán thành công ✅");
        }
        else
        {
            return Content("Thanh toán thất bại ❌");
        }
    }

    private string HmacSHA512(string key, string inputData)
    {
        var hash = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hashBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(inputData));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}