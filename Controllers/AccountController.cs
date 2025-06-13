using Microsoft.AspNetCore.Mvc;
using E_ticaret_Sitesi.Models;
using System.Text;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;


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
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }

    // GET: /Account/Register
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Register(User user)
    {
        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("", "Bu e-posta zaten kayıtlı.");
            return View();
        }

        user.PasswordHash = ComputeSha256Hash(user.PasswordHash);
        user.Role = "User";
        user.CreatedAt = DateTime.Now;

        _context.Users.Add(user);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }

    // GET: /Account/Login
    public IActionResult Login() => View();

    // POST: /Account/Login
    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        var hashedPassword = ComputeSha256Hash(password);

        if (user != null && user.PasswordHash == hashedPassword)
        {
            //  Cookie için claim listesi
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User") 
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Cookie oluştur
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Geçersiz e-posta veya şifre.";
        return View();
    }

    // /Account/Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

}
