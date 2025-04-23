using Microsoft.AspNetCore.Mvc;
using E_ticaret_Sitesi.Models;
using Microsoft.EntityFrameworkCore;

namespace E_ticaret_Sitesi.Controllers
{
    public class CartController : Controller
    {
        private readonly OnlineShoppingContext _context;

        public CartController(OnlineShoppingContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Kullanıcının sepetini bul (yoksa oluştur)
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            // productId'nin doğru alındığını kontrol et
            Console.WriteLine($"Sepete Eklenen Ürün ID: {productId}");

            // Eğer productId geçerli değilse, hata döndürelim
            if (productId <= 0)
            {
                return Json(new { success = false, message = "Geçersiz ürün ID'si." });
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Giriş yapmalısınız." });

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var existingItem = _context.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = 1
                });
            }

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("DbUpdateException: " + ex.InnerException?.Message);
                return Json(new { success = false, message = "Bir hata oluştu." });
            }

            return Json(new { success = true });
        }



    }
}
