using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_ticaret_Sitesi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using E_ticaret_Sitesi.Models;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using E_ticaret_Sitesi.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
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
            QuestPDF.Settings.License = LicenseType.Community;

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);

                    page.Header().Text("FATURA").FontSize(24).Bold().AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Sipariş No: {order.OrderId}").FontSize(12);
                        col.Item().Text($"Tarih: {order.OrderDate?.ToString("dd.MM.yyyy HH:mm") ?? "Bilinmiyor"}").FontSize(12);
                        col.Item().Text($"Müşteri: {order.User?.FullName ?? "Bilinmiyor"} ({order.User?.Email ?? "Email yok"})").FontSize(12);

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(); // Ürün
                                columns.ConstantColumn(50); // Adet
                                columns.ConstantColumn(80); // Fiyat
                                columns.ConstantColumn(90); // Toplam
                            });

                            // Başlık
                            table.Header(header =>
                            {
                                header.Cell().Text("Ürün").Bold();
                                header.Cell().Text("Adet").Bold();
                                header.Cell().Text("Fiyat").Bold();
                                header.Cell().Text("Toplam").Bold();
                            });

                            // Satırlar
                            foreach (var item in order.OrderDetails)
                            {
                                table.Cell().Text(item.Product?.Name ?? "Bilinmiyor");
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text($"{item.UnitPrice.ToString("C", new System.Globalization.CultureInfo("tr-TR"))}");
                                table.Cell().Text($"{(item.UnitPrice * item.Quantity).ToString("C", new System.Globalization.CultureInfo("tr-TR"))}");
                            }
                        });

                        col.Item().AlignRight().PaddingTop(10)
                            .Text($"Genel Toplam: {order.TotalAmount.ToString("C", new System.Globalization.CultureInfo("tr-TR"))}")
                            .Bold().FontSize(14);
                    });

                    page.Footer().AlignCenter().Text("Bizi tercih ettiğiniz için teşekkür ederiz!").Italic();
                });
            });

            return document.GeneratePdf();
        }

    }

    [HttpGet]
    [Authorize]
    public IActionResult Checkout()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var cart = _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            return RedirectToAction("Index", "Cart");

        var addresses = _context.Addresses.Where(a => a.UserId == userId).ToList();

        var model = new CheckoutViewModel
        {
            Cart = cart,
            AddressList = addresses.Select(a => new SelectListItem
            {
                Value = a.AddressId.ToString(),
                Text = $"{a.Title} - {a.FullName} ({a.City}/{a.District})"
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Checkout(CheckoutViewModel model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // HER ZAMAN CART YÜKLE
        var cart = _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefault(c => c.UserId == userId);
        model.Cart = cart;

        if (!ModelState.IsValid)
        {
            var addresses = _context.Addresses.Where(a => a.UserId == userId).ToList();
            model.AddressList = addresses.Select(a => new SelectListItem
            {
                Value = a.AddressId.ToString(),
                Text = $"{a.Title} - {a.FullName} ({a.City}/{a.District})"
            }).ToList();

            return View(model);
        }

        if (cart == null || !cart.CartItems.Any())
            return RedirectToAction("Index", "Cart");

        var selectedAddress = _context.Addresses.FirstOrDefault(a => a.AddressId == model.SelectedAddressId);

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = "Hazırlanıyor",
            TotalAmount = cart.CartItems.Sum(i => i.Quantity * i.Product.Price)
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        foreach (var item in cart.CartItems)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
            if (product != null)
            {
                product.Stock -= item.Quantity;
                if (product.Stock < 0) product.Stock = 0;
            }

            _context.OrderDetails.Add(new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price
            });
        }

        _context.CartItems.RemoveRange(cart.CartItems);
        _context.SaveChanges();

        TempData["OrderSuccess"] = "Siparişiniz başarıyla oluşturuldu!";
        return RedirectToAction("MyOrders");
    }


    public IActionResult MyOrders()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return RedirectToAction("Login", "Account");

        var orders = _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View(orders);
    }
    [HttpPost]
    [Authorize]
    public IActionResult CancelOrder(int id)
    {
        var order = _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefault(o => o.OrderId == id && o.Status == "Hazırlanıyor");

        if (order == null)
        {
            TempData["OrderMessage"] = "İptal edilemedi.";
            return RedirectToAction("MyOrders");
        }

        //  Stokları geri artır
        foreach (var item in order.OrderDetails)
        {
            if (item.Product != null)
            {
                item.Product.Stock += item.Quantity;
            }
        }

        order.Status = "İptal Edildi";
        _context.SaveChanges();

        TempData["OrderMessage"] = "Sipariş başarıyla iptal edildi ve stok güncellendi.";
        return RedirectToAction("MyOrders");
    }


    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult ReturnOrder(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var order = _context.Orders.FirstOrDefault(o => o.OrderId == id && o.UserId == userId);

        if (order == null || order.Status != "Teslim Edildi")
        {
            return NotFound();
        }

        order.Status = "İade Edildi";
        _context.SaveChanges();

        TempData["OrderSuccess"] = "Siparişiniz iade edildi.";
        return RedirectToAction("MyOrders");
    }


    [HttpGet]
    public IActionResult DownloadInvoice(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
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
