using E_ticaret_Sitesi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_ticaret_Sitesi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly OnlineShoppingContext _context;

        public AdminUsersController(OnlineShoppingContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult UpdateRole(int id, string newRole)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.Role = newRole;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }

}
