using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_ticaret_Sitesi.Models;
using Microsoft.AspNetCore.Authorization;

namespace E_ticaret_Sitesi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly OnlineShoppingContext _context;

        public AdminOrdersController(OnlineShoppingContext context)
        {
            _context = context;
        }

        // Sipariş listesi
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // Sipariş durumu güncelleme
        [HttpPost]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
