using System;

namespace MiniShop.Models;

public class Comment
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public string Content { get; set; }
    public int Rating { get; set; } // 1–5 sao

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}