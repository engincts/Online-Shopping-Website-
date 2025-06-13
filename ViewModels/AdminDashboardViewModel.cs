using Microsoft.AspNetCore.Mvc;

namespace E_ticaret_Sitesi.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
    }

}
