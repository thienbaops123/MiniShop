using System.ComponentModel.DataAnnotations;

namespace MiniShop.Models;

public class User
{
    public int Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required, StringLength(100)]
    public string FullName { get; set; } = "";
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public string? BanReason { get; set; }
    public DateTime? BannedUntil { get; set; }

    [Required]
    public string Role { get; set; } = "User";
}