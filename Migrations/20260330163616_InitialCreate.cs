using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MiniShop.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    BanReason = table.Column<string>(type: "TEXT", nullable: true),
                    BannedUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ShippingFullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShippingPhone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ShippingAddress = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Books" },
                    { 2, "Clothes" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "BanReason", "BannedUntil", "Email", "FullName", "PasswordHash", "Phone", "Role", "Status" },
                values: new object[] { 1, null, null, null, "admin@minishop.local", "MiniShop Admin", "$2a$11$SwZb3xZOEJYsRFTbBvJ.w.h2EXDjXX34epBkEnv1i3HXWUew8498C", null, "Admin", 0 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, "/images/products/book01.png", "Clean Code", 12.50m },
                    { 2, 2, "/images/products/clothes01.png", "T-Shirt Basic", 9.99m },
                    { 3, 1, "/images/products/book02.png", "The Pragmatic Programmer", 14.90m },
                    { 4, 1, "/images/products/book03.png", "Refactoring", 18.00m },
                    { 5, 1, "/images/products/book04.png", "Design Patterns", 20.00m },
                    { 6, 1, "/images/products/book05.png", "Domain-Driven Design", 22.50m },
                    { 7, 1, "/images/products/book06.png", "You Don't Know JS", 11.00m },
                    { 8, 1, "/images/products/book07.png", "Eloquent JavaScript", 10.50m },
                    { 9, 1, "/images/products/book08.png", "C# in Depth", 16.80m },
                    { 10, 1, "/images/products/book09.png", "ASP.NET Core in Action", 17.25m },
                    { 11, 1, "/images/products/book10.png", "Head First Design Patterns", 15.75m },
                    { 12, 1, "/images/products/book11.png", "The Clean Coder", 12.90m },
                    { 13, 1, "/images/products/book12.png", "Working Effectively with Legacy Code", 19.40m },
                    { 14, 2, "/images/products/clothes02.png", "Oversized T-Shirt", 11.50m },
                    { 15, 2, "/images/products/clothes03.png", "Hoodie Zip", 19.99m },
                    { 16, 2, "/images/products/clothes04.png", "Hoodie Pullover", 18.50m },
                    { 17, 2, "/images/products/clothes05.png", "Jeans Slim Fit", 24.90m },
                    { 18, 2, "/images/products/clothes06.png", "Jeans Regular", 23.50m },
                    { 19, 2, "/images/products/clothes07.png", "Chino Pants", 21.00m },
                    { 20, 2, "/images/products/clothes08.png", "Jacket Denim", 29.90m },
                    { 21, 2, "/images/products/clothes09.png", "Jacket Bomber", 32.00m },
                    { 22, 2, "/images/products/clothes10.png", "Sweater Knit", 16.90m },
                    { 23, 2, "/images/products/clothes11.png", "Shorts Casual", 12.00m },
                    { 24, 2, "/images/products/clothes12.png", "Polo Shirt", 13.90m },
                    { 25, 2, "/images/products/clothes13.png", "Cap Classic", 7.50m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
