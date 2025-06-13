using E_ticaret_Sitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using E_ticaret_Sitesi.ViewModels;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly OnlineShoppingContext _context;

    public AdminController(OnlineShoppingContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Dashboard()
    {
        var viewModel = new AdminDashboardViewModel
        {
            TotalUsers = _context.Users.Count(),
            TotalProducts = _context.Products.Count(),
            TotalOrders = _context.Orders.Count(),
            TotalRevenue = _context.Orders.Sum(o => o.TotalAmount),

            MonthlyRevenue = _context.Orders
                .Where(o => o.OrderDate.HasValue)
                .AsEnumerable() // EF'den sonra belleğe al
                .GroupBy(o => new
                {
                    Year = o.OrderDate.Value.Year,
                    Month = o.OrderDate.Value.Month
                })
                .Select(g => new MonthlyRevenueDto
                {
                    Month = $"{g.Key.Month:00}.{g.Key.Year}",
                    Revenue = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(g => g.Month)
                .ToList()
        };

        return View(viewModel);
    }

}
