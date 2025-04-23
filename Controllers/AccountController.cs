using Microsoft.AspNetCore.Mvc;
using E_ticaret_Sitesi.Models;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using System.Linq;

public class AccountController : Controller
{
    private readonly OnlineShoppingContext _context;

    public AccountController(OnlineShoppingContext context)
    {
        _context = context;
    }

    // 🔐 SHA256 Hash Fonksiyonu
    public static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    // GET: /Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    public IActionResult Register(User user)
    {
        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("", "Bu e-posta zaten kayıtlı.");
            return View();
        }

        user.PasswordHash = ComputeSha256Hash(user.PasswordHash); // 👈 senin hash fonksiyonun
        user.CreatedAt = DateTime.Now;

        _context.Users.Add(user);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }

    // GET: /Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        string hashedPassword = ComputeSha256Hash(password); // 👈 kullanıcıdan gelen şifre hashleniyor

        if (user != null && user.PasswordHash == hashedPassword)
        {
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName);
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Geçersiz e-posta veya şifre.";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
