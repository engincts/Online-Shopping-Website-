using Microsoft.AspNetCore.Mvc;
using E_ticaret_Sitesi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace E_ticaret_Sitesi.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly OnlineShoppingContext _context;

        public CartController(OnlineShoppingContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim.Value);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return View(cart);
        }


        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            Console.WriteLine($"Sepete Eklenen Ürün ID: {productId}");

            if (productId <= 0)
                return RedirectToAction("Index", "Home");

            // Claims üzerinden kullanıcı ID'si alınıyor
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login", "Account");

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
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
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public IActionResult RemoveItem(int cartItemId)
        {
            var item = _context.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }


    }
}
