using MiniShop.Models;

namespace MiniShop.Models.ViewModels;

public class ProductIndexVm
{
    public List<Product> Items { get; set; } = new();

    public int? CategoryId { get; set; }
    public string? Query { get; set; }
    public string Sort { get; set; } = "new";

    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public List<Category> Categories { get; set; } = new();
}