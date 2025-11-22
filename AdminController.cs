using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VintageCafe.Data;
using VintageCafe.Models;

namespace VintageCafe.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private const string ADMIN_SESSION = "IsAdmin";  // Session key for admin login

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // ✅ Helper: Check Admin Session
        private bool IsAdmin() => HttpContext.Session.GetString(ADMIN_SESSION) == "true";

        // ✅ Login Page
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Admin credentials
            if (email == "JhalakR@vintagecafe.com" && password == "jhalak25")
            {
                HttpContext.Session.SetString(ADMIN_SESSION, "true");
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid credentials. Try again.";
            return View();
        }

        // ✅ Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(ADMIN_SESSION);
            return RedirectToAction("Login");
        }

        // ✅ Admin Dashboard
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            ViewBag.DineIn = _db.Orders.Count(o => o.IsDineIn);
            ViewBag.TakeAway = _db.Orders.Count(o => o.IsTakeAway);
            ViewBag.TotalSales = _db.Orders.Sum(o => (decimal?)o.Total) ?? 0m;
            return View();
        }

        // ✅ Manage Menu
        public IActionResult ManageMenu()
        {
            if (!IsAdmin()) return RedirectToAction("Login");
            var items = _db.MenuItems.ToList();
            return View(items);
        }

        [HttpPost]
        public IActionResult AddMenu(string name, decimal price, string category = "General", bool isSnack = false, string imagePath = "/images/foodsimg/placeholder.png")
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(name) || price <= 0)
            {
                TempData["Error"] = "Please enter a valid name and price.";
                return RedirectToAction("ManageMenu");
            }

            var newItem = new MenuItem
            {
                Name = name,
                Price = price,
                Category = category,
                IsSnack = isSnack,
                ImagePath = imagePath
            };

            _db.MenuItems.Add(newItem);
            _db.SaveChanges();
            TempData["Msg"] = $"✅ '{name}' added successfully!";
            return RedirectToAction("ManageMenu");
        }

        [HttpPost]
        public IActionResult RemoveMenu(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            var item = _db.MenuItems.Find(id);
            if (item != null)
            {
                _db.MenuItems.Remove(item);
                _db.SaveChanges();
                TempData["Msg"] = $"🗑️ '{item.Name}' removed.";
            }

            return RedirectToAction("ManageMenu");
        }

        // ✅ View Orders
        public IActionResult Orders()
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            var orders = _db.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.MenuItem)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        // ✅ Update Order Status (Fixes CS0103/CS0106)
        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string actionType)
        {
            if (!IsAdmin()) return RedirectToAction("Login");

            var order = _db.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return NotFound();

            order.Status = actionType == "approve" ? "Approved" : "Denied";
            _db.SaveChanges();

            TempData["Msg"] = $"Order #{order.Id} marked as {order.Status}.";
            return RedirectToAction("Orders");
        }
    }
}
