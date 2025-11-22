using Microsoft.AspNetCore.Mvc;
using VintageCafe.Data;
using VintageCafe.Models;
using Microsoft.EntityFrameworkCore;
using VintageCafe.Helpers;

namespace VintageCafe.Controllers
{
    public class CafeController : Controller
    {
        private readonly AppDbContext _db;
        private const string CART_KEY = "cart";
        private const string MODE_KEY = "mode";

        public CafeController(AppDbContext db)
        {
            _db = db;
        }

        // ✅ MENU PAGE (with welcome + order mode popups)
        public async Task<IActionResult> Menu()
        {
            var isLoggedIn = User?.Identity?.IsAuthenticated ?? false;

            if (!isLoggedIn)
            {
                TempData["LoginPrompt"] = true;
            }
            else
            {
                TempData["WelcomeName"] = User.Identity?.Name ?? "Guest";
            }

            var mode = HttpContext.Session.GetString(MODE_KEY);
            if (isLoggedIn && string.IsNullOrEmpty(mode))
            {
                TempData["AskOrderMode"] = true;
            }

            var items = await _db.MenuItems
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();

            ViewBag.Mode = mode ?? "Not Selected";
            return View(items);
        }

        // ✅ Set order mode (Dine-in / Take-away)
        [HttpPost]
        public IActionResult SetMode(string mode)
        {
            if (!string.IsNullOrEmpty(mode))
                HttpContext.Session.SetString(MODE_KEY, mode);

            return RedirectToAction("Menu");
        }

        // ✅ Add item to cart (AJAX)
        [HttpPost]
        public JsonResult AddToCart(int id)
        {
            try
            {
                if (!(User.Identity?.IsAuthenticated ?? false))
                    return Json(new { ok = false, loginRequired = true });

                var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>(CART_KEY)
                           ?? new Dictionary<int, int>();

                var item = _db.MenuItems.Find(id);
                if (item == null)
                    return Json(new { ok = false, message = "Item not found" });

                cart[id] = cart.GetValueOrDefault(id, 0) + 1;
                HttpContext.Session.SetObjectAsJson(CART_KEY, cart);

                return Json(new { ok = true });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ✅ View Cart
        public IActionResult Cart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>(CART_KEY)
                       ?? new Dictionary<int, int>();

            if (cart.Count == 0)
                return View(new CartPageViewModel());

            return View(CreateCartModel(cart));
        }

        // ✅ UpdateCartAjax (for +, -, remove) — fully works now
        [HttpPost]
        public JsonResult UpdateCartAjax(string actionType, int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>(CART_KEY)
                       ?? new Dictionary<int, int>();

            if (!_db.MenuItems.Any(m => m.Id == id))
                return Json(new { ok = false, message = "Item not found" });

            switch (actionType?.ToLower())
            {
                case "inc":
                    cart[id] = cart.GetValueOrDefault(id, 0) + 1;
                    break;

                case "dec":
                    if (cart.ContainsKey(id))
                    {
                        cart[id]--;
                        if (cart[id] <= 0)
                            cart.Remove(id);
                    }
                    break;

                case "remove":
                    cart.Remove(id);
                    break;
            }

            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            var model = CreateCartModel(cart);

            return Json(new
            {
                ok = true,
                items = model.Items.Select(i => new
                {
                    id = i.MenuItem.Id,
                    name = i.MenuItem.Name,
                    qty = i.Quantity,
                    price = i.MenuItem.Price,
                    lineTotal = i.LineTotal
                }),
                subTotal = model.SubTotal,
                gst = model.GST,
                total = model.Total
            });
        }

        // ✅ Clear Cart
        [HttpPost]
        public JsonResult ClearCart()
        {
            HttpContext.Session.Remove(CART_KEY);
            return Json(new { ok = true });
        }

        // ✅ Checkout
        [HttpPost]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>(CART_KEY)
                       ?? new Dictionary<int, int>();

            if (cart.Count == 0)
                return RedirectToAction("Menu");

            HttpContext.Session.Remove(CART_KEY);
            return RedirectToAction("Menu");
        }

        // ✅ Helper: Build Cart Model
        private CartPageViewModel CreateCartModel(Dictionary<int, int> cart)
        {
            var vm = cart.Select(kvp =>
            {
                var item = _db.MenuItems.Find(kvp.Key);
                return new CartViewModel
                {
                    MenuItem = item!,
                    Quantity = kvp.Value,
                    LineTotal = (item?.Price ?? 0) * kvp.Value
                };
            }).Where(x => x.MenuItem != null).ToList();

            decimal sub = vm.Sum(x => (decimal)x.LineTotal);
            decimal gst = Math.Round(sub * 0.05m, 2);
            decimal total = sub + gst;

            return new CartPageViewModel
            {
                Items = vm,
                SubTotal = sub,
                GST = gst,
                Total = total
            };
        }
    }

    // ✅ Cart View Models
    public class CartViewModel
    {
        public MenuItem MenuItem { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CartPageViewModel
    {
        public List<CartViewModel> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal GST { get; set; }
        public decimal Total { get; set; }
    }
}
