using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_ticaret_Sitesi.Models;
using System.Linq;

namespace E_ticaret_Sitesi.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OnlineShoppingContext _context;

        public ProductsController(OnlineShoppingContext context)
        {
            _context = context;
        }

        // Ürün Listeleme
        public IActionResult Index(int? categoryId, string? search)
        {
            var categories = _context.Categories.ToList(); // Tüm kategorileri al
            ViewBag.Categories = categories;

            var products = _context.Products.AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p => p.Name.Contains(search));
            }

            return View(products.ToList());
        }



        // Ürün Detay
        public IActionResult Details(int id)
        {
            var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [Route("Products/Category/{categoryId}")]
        public IActionResult Category(int categoryId)
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;

            var products = _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToList();

            return View("Index", products); // Index.cshtml view'ını kullan
        }

    }
}
