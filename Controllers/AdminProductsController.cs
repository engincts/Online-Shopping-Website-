using Microsoft.AspNetCore.Mvc;
using E_ticaret_Sitesi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace E_ticaret_Sitesi.Controllers
{
    public class AdminProductsController : Controller
    {
        private readonly OnlineShoppingContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Dosya yükleme için

        public AdminProductsController(OnlineShoppingContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. Ürünleri Listele
        public IActionResult Index()
        {
            var products = _context.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        // 2. Ürün Ekle - GET
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                var maxFileSize = 5 * 1024 * 1024; // 5MB
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                if (imageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("ImageFile", "Dosya boyutu çok büyük. Lütfen 5MB'den küçük dosya yükleyin.");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View();
                }

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageFile", "Geçersiz dosya türü. Lütfen .jpg veya .png dosyası yükleyin.");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View();
                }

                try
                {
                    // wwwroot/Pictures klasörünü hedef alıyoruz
                    var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "Pictures");

                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory);
                    }

                    // Orijinal dosya adını al
                    var originalFileName = Path.GetFileName(imageFile.FileName);

                    // Eğer aynı isimli dosya varsa onu kullan, yoksa yeni GUID ile oluştur
                    var fileName = originalFileName;
                    var uploadPath = Path.Combine(uploadDirectory, fileName);

                    if (System.IO.File.Exists(uploadPath))
                    {
                        // Dosya zaten varsa — tekrar yükleme
                        Console.WriteLine("Dosya zaten var, tekrar yüklenmedi.");
                    }
                    else
                    {
                        // Dosyayı yükle
                        using (var stream = new FileStream(uploadPath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                    }

                    // ImageUrl’yi kaydet (web için geçerli yol)
                    product.ImageUrl = "/Pictures/" + fileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Resim yüklenirken hata oluştu: {ex.Message}");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View();
                }
            }

            // Ürünü veritabanına ekle
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }




        // 3. Ürün Güncelle - GET
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // 3. Ürün Güncelle - POST
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // 4. Ürün Sil
        public IActionResult Delete(int id)
        {
            var product = _context.Products
                                  .Include(p => p.CartItems)     // Sepet öğeleri
                                  .Include(p => p.OrderDetails)   // Sipariş detayları
                                  .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound(); // Ürün bulunamazsa hata döndür
            }

            // Sepet öğelerini ve sipariş detaylarını sil
            _context.CartItems.RemoveRange(product.CartItems);
            _context.OrderDetails.RemoveRange(product.OrderDetails);

            // Ürünü sil
            _context.Products.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

    }
}
