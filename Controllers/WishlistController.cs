using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using E_ticaret_Sitesi.Models;
using Microsoft.EntityFrameworkCore;

namespace E_ticaret_Sitesi.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly OnlineShoppingContext _context;

        public WishlistController(OnlineShoppingContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var wishlist = _context.Wishlists
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .ToList();

            return View(wishlist);
        }

        [HttpPost]
        public IActionResult Add(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (!_context.Wishlists.Any(w => w.UserId == userId && w.ProductId == productId))
            {
                _context.Wishlists.Add(new Wishlist { UserId = userId, ProductId = productId });
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        [Authorize]
        public IActionResult MoveToWishlist(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Zaten varsa ekleme
            var exists = _context.Wishlists.Any(w => w.UserId == userId && w.ProductId == productId);
            if (!exists)
            {
                var wishlist = new Wishlist
                {
                    UserId = userId,
                    ProductId = productId
                };
                _context.Wishlists.Add(wishlist);
            }

            // Sepetten çıkar
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart != null)
            {
                var itemToRemove = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (itemToRemove != null)
                {
                    _context.CartItems.Remove(itemToRemove);
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var item = _context.Wishlists.FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

            if (item != null)
            {
                _context.Wishlists.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }

}
