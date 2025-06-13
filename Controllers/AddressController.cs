using Microsoft.AspNetCore.Mvc;
using E_ticaret_Sitesi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;

[Authorize]
public class AddressController : Controller
{
    private readonly OnlineShoppingContext _context;

    public AddressController(OnlineShoppingContext context)
    {
        _context = context;
    }

    // [GET] /Address
    public IActionResult Index()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var addresses = _context.Addresses.Where(a => a.UserId == userId).ToList();
        return View(addresses);
    }

    // [GET] /Address/Create
    public IActionResult Create()
    {
        return View();
    }

    // [POST] /Address/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Address address)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        address.UserId = userId;

        foreach (var kvp in ModelState)
        {
            foreach (var error in kvp.Value.Errors)
            {
                Console.WriteLine($"Property: {kvp.Key}, Error: {error.ErrorMessage}");
            }
        }


        if (ModelState.IsValid)
        {
            _context.Addresses.Add(address);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        return View(address);
    }
}
