using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;
using System.Net.Http;

namespace WebSellingShoes.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }

        /*public IActionResult Index()
        {
            var products = _dataContext.Products.Include("Category").Include("Brand").ToList();

            var sliders = _dataContext.Sliders.Where(x => x.Status == 1).ToList();
            ViewBag.Sliders = sliders;

            return View(products);

           *//* var products = _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Category.Status == 1 && p.Brand.Status == 1)
                .ToList();

            var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
            ViewBag.Sliders = sliders;

            var categories = _dataContext.Categories.Where(c => c.Status == 1).ToList();
            ViewBag.Categories = categories;

            var brands = _dataContext.Brands.Where(b => b.Status == 1).ToList();
            ViewBag.Brands = brands;

            return View(products);*//*
        }*/
        public async Task<IActionResult> Index(int pg = 1)
        {
            var sliders = _dataContext.Sliders.Where(x => x.Status == 1).ToList();
            ViewBag.Sliders = sliders;
            const int pageSize = 9; // 9 sa?n ph�?m m??i trang
            if (pg < 1)
            {
                pg = 1; // ?a?mm ba?o trang kh�ng nho? h?n 1
            }

            // l�?y t??ng s�? sa?n ph�?m t?? database
            int recsCount = await _dataContext.Products.CountAsync(); // t??ng s�? sa?n ph�?m

            // T?o ??i t??ng Paginate
            var pager = new Paginate(recsCount, pg, pageSize);

            // L?y d? li?u cho trang hi?n t?i
            var products = await _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderBy(p => p.Id) // S?p x?p ?? ??m b?o th? t? nh?t qu�n
                .Skip((pg - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truy?n ??i t??ng Paginate v�o ViewBag
            ViewBag.Pager = pager;

            return View(products);
        }


        public async Task<IActionResult> Compare()
        {
            /*var compare_product = await (from c in _dataContext.Compares
                                         join p in _dataContext.Products on c.ProductId equals p.Id
                                         join u in _dataContext.Users on c.UserId equals u.Id
                                         select new { User = u, Product = p, Compares = c })
                               .ToListAsync();

            return View(compare_product);*/

            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("UserId: {UserId}", userId); // Ghi log ?? ki?m tra

            var compares = await _dataContext.Compares
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(compares);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Json(new { success = false, message = "Vui l�ng ??ng nh�?p ?�? th??c hi�?n thao t�c n�y." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Kh�ng th�? x�c ?i?nh ng???i d�ng." });
            }

            var compare = await _dataContext.Compares
                .FirstOrDefaultAsync(c => c.ProductId == Id && c.UserId == userId);

            if (compare == null)
            {
                return Json(new { success = false, message = "Sa?n ph�?m kh�ng t�?n ta?i trong danh s�ch so s�nh." });
            }

            try
            {
                _dataContext.Compares.Remove(compare);
                await _dataContext.SaveChangesAsync();
                return Json(new { success = true, message = "Sa?n ph�?m ?� ????c x�a kho?i danh s�ch so s�nh." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L�?i khi x�a sa?n ph�?m so s�nh v??i ProductId {ProductId} v� UserId {UserId}", Id, userId);
                return Json(new { success = false, message = "?� xa?y ra l�?i khi x�a sa?n ph�?m. Vui l�ng th?? la?i." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            /*WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);

            _dataContext.Wishlists.Remove(wishlist);

            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Y�u th�ch ?a? ????c x�a th�nh c�ng";
            return RedirectToAction("Wishlist", "Home");*/
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Unauthorized();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var wishlist = await _dataContext.Wishlists
                .FirstOrDefaultAsync(c => c.ProductId == Id && c.UserId == userId);

            if (wishlist != null)
            {
                _dataContext.Wishlists.Remove(wishlist);
                await _dataContext.SaveChangesAsync();
                return Json(new { success = true, message = "Sa?n ph�?m bi? lo?ai bo? kho?i danh s�ch y�u thi?ch." });
            }

            return Json(new { success = false, message = "Sa?n ph�?m kh�ng t�m th�?y trong danh s�ch y�u thi?ch." });

        }
        public async Task<IActionResult> Wishlist()
        {
            /*var wishlist_product = await (from w in _dataContext.Wishlists
                                          join p in _dataContext.Products on w.ProductId equals p.Id
                                          select new { Product = p, Wishlists = w })
                               .ToListAsync();

            return View(wishlist_product);*/
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var wishlists = await _dataContext.Wishlists
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return View(wishlists);
        }

        [HttpPost]
        public async Task<IActionResult> AddWishlist(int Id)//, WishlistModel wishlistmodel
        {
            /*var user = await _userManager.GetUserAsync(User);

            var wishlistProduct = new WishlistModel
            {
                ProductId = Id,
                UserId = user.Id
            };

            _dataContext.Wishlists.Add(wishlistProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Th�m va?o y�u thi?ch tha?nh c�ng" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding to wishlist table.");
            }*/
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Unauthorized();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingWishlist = await _dataContext.Wishlists
                .FirstOrDefaultAsync(w => w.ProductId == Id && w.UserId == userId);

            if (existingWishlist == null)
            {
                var wishlist = new WishlistModel
                {
                    ProductId = Id,
                    UserId = userId
                };

                _dataContext.Wishlists.Add(wishlist);
                await _dataContext.SaveChangesAsync();
                return Json(new { success = true, message = "?a? th�m sa?n ph�?m va?o mu?c y�u thi?ch." });
            }

            return Json(new { success = false, message = "Sa?n ph�?m ?a? t�?n ta?ii trong mu?c y�u thi?ch." });


        }
        [HttpPost]
        public async Task<IActionResult> AddCompare(int Id)
        {
            /*var user = await _userManager.GetUserAsync(User);

            var compareProduct = new CompareModel
            {
                ProductId = Id,
                UserId = user.Id
            };

            _dataContext.Compares.Add(compareProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Add to compare Successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding to compare table.");
            }*/
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return Unauthorized();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var compareCount = await _dataContext.Compares
                .CountAsync(c => c.UserId == userId);

            if (compareCount >= 2)
            {
                return Json(new { success = false, message = "Ba?n chi? co? th�? th�m 2 sa?n ph�?m va?o mu?c so sa?nh." });
            }

            var existingCompare = await _dataContext.Compares
                .FirstOrDefaultAsync(c => c.ProductId == Id && c.UserId == userId);

            if (existingCompare == null)
            {
                var compare = new CompareModel
                {
                    ProductId = Id,
                    UserId = userId
                };

                _dataContext.Compares.Add(compare);
                await _dataContext.SaveChangesAsync();
                return Json(new { success = true, message = "?a? th�m sa?n ph�?m va?o mu?c so sa?nh." });
            }

            return Json(new { success = false, message = "Sa?n ph�?m ?a? t�?n ta?i trong so sa?nh." });


        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            } 
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contacts.FirstAsync();
            return View(contact);
        }

        /*[HttpPost]
        public async Task<IActionResult> ChatBot([FromBody] string message)
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new StringContent(
                JsonSerializer.Serialize(new { user_input = message }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync("h ttp://lo calhost:5000/chat", payload);
            var responseString = await response.Content.ReadAsStringAsync();

            using JsonDocument json = JsonDocument.Parse(responseString);
            var reply = json.RootElement.GetProperty("response").GetString();

            // Ki?m tra n?u ng??i d�ng ??ng � th�m s?n ph?m v�o gi? h�ng
            if (reply.Contains("??ng �") || reply.Contains("ch?p nh?n"))
            {
                // Th�m s?n ph?m v�o gi? h�ng
                var productId = 1; // ID s?n ph?m c?n th�m
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cartItem = new CartItemsModels
                {
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 1
                };
                _dataContext.Cart.Add(cartItem);
                await _dataContext.SaveChangesAsync();
            }

            return Json(new { reply });
        }
*/
    }
}
