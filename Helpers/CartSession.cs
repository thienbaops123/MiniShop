using MiniShop.Models;

namespace MiniShop.Helpers;

public static class CartSession
{
    public const string Key = "CART";

    public static List<CartItem> GetCart(HttpContext http)
        => http.Session.GetObject<List<CartItem>>(Key) ?? new List<CartItem>();

    public static void SaveCart(HttpContext http, List<CartItem> cart)
        => http.Session.SetObject(Key, cart);

    public static void Clear(HttpContext http)
        => http.Session.Remove(Key);
}