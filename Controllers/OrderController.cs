using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_ticaret_Sitesi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using E_ticaret_Sitesi.Models;
using System.Reflection.Metadata;

public class OrderController : Controller
{
    private readonly OnlineShoppingContext _context;

    public OrderController(OnlineShoppingContext context)
    {
        _context = context;
    }

    public class InvoiceService
    {
        public static byte[] GenerateInvoicePdf(Order order)
        {
            // Lisans tipini belirtmek için bu satırı ekleyin
            QuestPDF.Settings.License = LicenseType.Community;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    page.Header().Text("FATURA").FontSize(24).Bold().AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Sipariş No: {order.OrderId}");
                        col.Item().Text($"Tarih: {order.OrderDate?.ToString("dd.MM.yyyy HH:mm")}");
                        col.Item().Text($"Müşteri: {order.User?.FullName} ({order.User?.Email})");

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(); // Ürün ismi için dinamik genişlik
                                columns.RelativeColumn(); // Adet için dinamik genişlik
                                columns.RelativeColumn(); // Fiyat için dinamik genişlik
                                columns.RelativeColumn(); // Toplam için dinamik genişlik
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Ürün").Bold();
                                header.Cell().Text("Adet").Bold();
                                header.Cell().Text("Fiyat").Bold();
                                header.Cell().Text("Toplam").Bold();
                            });

                            foreach (var item in order.OrderDetails)
                            {
                                table.Cell().Text(item.Product?.Name ?? "Bilgi Yok");
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text($"{item.UnitPrice} ₺");
                                table.Cell().Text($"{item.UnitPrice * item.Quantity} ₺");
                            }
                        });

                        col.Item().AlignRight().Text($"Toplam: {order.TotalAmount} ₺").Bold().FontSize(14);
                    });

                    page.Footer().AlignCenter().Text("Teşekkür ederiz!").Italic();
                });
            });

            return document.GeneratePdf();
        }
    }

    [HttpPost]
    public IActionResult Checkout()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Account");

        // Sepeti çek
        var cart = _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            return RedirectToAction("Index", "Cart");

        // Sipariş oluştur
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "Hazırlanıyor",
            TotalAmount = cart.CartItems.Sum(i => i.Quantity * i.Product.Price)
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        // Sipariş detayları
        foreach (var item in cart.CartItems)
        {
            var detail = new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price
            };
            _context.OrderDetails.Add(detail);
        }

        // Sepeti temizle
        _context.CartItems.RemoveRange(cart.CartItems);
        _context.SaveChanges();

        TempData["OrderSuccess"] = "Siparişiniz başarıyla oluşturuldu!";
        return RedirectToAction("MyOrders");
    }

    public IActionResult MyOrders()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var orders = _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View(orders);
    }

    [HttpGet]
    public IActionResult DownloadInvoice(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var order = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

        if (order == null)
            return NotFound();

        var pdfBytes = InvoiceService.GenerateInvoicePdf(order);

        return File(pdfBytes, "application/pdf", $"Fatura-{order.OrderId}.pdf");
    }

}
