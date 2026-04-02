using Microsoft.EntityFrameworkCore;
using MiniShop.Models;


namespace MiniShop.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Comment> Comments => Set<Comment>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Books" },
            new Category { Id = 2, Name = "Clothes" }
        );

        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "admin@minishop.local",
                FullName = "MiniShop Admin",
                Role = "Admin",
                PasswordHash = adminHash
            }
        );

        modelBuilder.Entity<Product>().HasData(
            // Books (CategoryId = 1)
            new Product { Id = 1, Name = "Clean Code", Price = 12.50m, CategoryId = 1, ImageUrl = "/images/products/book01.png" },
            new Product { Id = 3, Name = "The Pragmatic Programmer", Price = 14.90m, CategoryId = 1, ImageUrl = "/images/products/book02.png" },
            new Product { Id = 4, Name = "Refactoring", Price = 18.00m, CategoryId = 1, ImageUrl = "/images/products/book03.png" },
            new Product { Id = 5, Name = "Design Patterns", Price = 20.00m, CategoryId = 1, ImageUrl = "/images/products/book04.png" },
            new Product { Id = 6, Name = "Domain-Driven Design", Price = 22.50m, CategoryId = 1, ImageUrl = "/images/products/book05.png" },
            new Product { Id = 7, Name = "You Don't Know JS", Price = 11.00m, CategoryId = 1, ImageUrl = "/images/products/book06.png" },
            new Product { Id = 8, Name = "Eloquent JavaScript", Price = 10.50m, CategoryId = 1, ImageUrl = "/images/products/book07.png" },
            new Product { Id = 9, Name = "C# in Depth", Price = 16.80m, CategoryId = 1, ImageUrl = "/images/products/book08.png" },
            new Product { Id = 10, Name = "ASP.NET Core in Action", Price = 17.25m, CategoryId = 1, ImageUrl = "/images/products/book09.png" },
            new Product { Id = 11, Name = "Head First Design Patterns", Price = 15.75m, CategoryId = 1, ImageUrl = "/images/products/book10.png" },
            new Product { Id = 12, Name = "The Clean Coder", Price = 12.90m, CategoryId = 1, ImageUrl = "/images/products/book11.png" },
            new Product { Id = 13, Name = "Working Effectively with Legacy Code", Price = 19.40m, CategoryId = 1, ImageUrl = "/images/products/book12.png" },

            // Clothes (CategoryId = 2)
            new Product { Id = 2, Name = "T-Shirt Basic", Price = 9.99m, CategoryId = 2, ImageUrl = "/images/products/clothes01.png" },
            new Product { Id = 14, Name = "Oversized T-Shirt", Price = 11.50m, CategoryId = 2, ImageUrl = "/images/products/clothes02.png" },
            new Product { Id = 15, Name = "Hoodie Zip", Price = 19.99m, CategoryId = 2, ImageUrl = "/images/products/clothes03.png" },
            new Product { Id = 16, Name = "Hoodie Pullover", Price = 18.50m, CategoryId = 2, ImageUrl = "/images/products/clothes04.png" },
            new Product { Id = 17, Name = "Jeans Slim Fit", Price = 24.90m, CategoryId = 2, ImageUrl = "/images/products/clothes05.png" },
            new Product { Id = 18, Name = "Jeans Regular", Price = 23.50m, CategoryId = 2, ImageUrl = "/images/products/clothes06.png" },
            new Product { Id = 19, Name = "Chino Pants", Price = 21.00m, CategoryId = 2, ImageUrl = "/images/products/clothes07.png" },
            new Product { Id = 20, Name = "Jacket Denim", Price = 29.90m, CategoryId = 2, ImageUrl = "/images/products/clothes08.png" },
            new Product { Id = 21, Name = "Jacket Bomber", Price = 32.00m, CategoryId = 2, ImageUrl = "/images/products/clothes09.png" },
            new Product { Id = 22, Name = "Sweater Knit", Price = 16.90m, CategoryId = 2, ImageUrl = "/images/products/clothes10.png" },
            new Product { Id = 23, Name = "Shorts Casual", Price = 12.00m, CategoryId = 2, ImageUrl = "/images/products/clothes11.png" },
            new Product { Id = 24, Name = "Polo Shirt", Price = 13.90m, CategoryId = 2, ImageUrl = "/images/products/clothes12.png" },
            new Product { Id = 25, Name = "Cap Classic", Price = 7.50m, CategoryId = 2, ImageUrl = "/images/products/clothes13.png" }
        );
    }
}