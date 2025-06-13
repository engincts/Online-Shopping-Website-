using E_ticaret_Sitesi.Models; // �r�n modeli
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace E_ticaret_Sitesi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly OnlineShoppingContext _context;

        public HomeController(ILogger<HomeController> logger, OnlineShoppingContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int? categoryId)
        {
            ViewBag.Categories = _context.Categories.ToList();

            var productsQuery = _context.Products
                .Where(p => !categoryId.HasValue || p.CategoryId == categoryId)
                .OrderByDescending(p => p.ProductId)
                .Select(p => new Product
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    Category = p.Category
                });

            var products = productsQuery.ToList();

            return View(products);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
