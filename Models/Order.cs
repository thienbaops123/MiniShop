using System.ComponentModel.DataAnnotations;

namespace MiniShop.Models;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal TotalAmount { get; set; }

    [Required, StringLength(100)]
    public string ShippingFullName { get; set; } = "";

    [Required, StringLength(30)]
    public string ShippingPhone { get; set; } = "";

    [Required, StringLength(300)]
    public string ShippingAddress { get; set; } = "";

    public List<OrderItem> Items { get; set; } = new();
}