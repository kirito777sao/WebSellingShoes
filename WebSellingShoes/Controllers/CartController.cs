using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebSellingShoes.Models;
using WebSellingShoes.Models.ViewModels;
using WebSellingShoes.Repository;

namespace WebSellingShoes.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;
        public CartController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index(ShippingModel shippingModel)
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            // Nhận shipping giá từ cookie
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }

            // Nhận Coupon code từ cookie
            var coupon_code = Request.Cookies["CouponTitle"];

            // Nhận Coupon percent từ cookie
            var couponPercentCookie = Request.Cookies["CouponPercent"];
            int couponPercent = 0;

            if (couponPercentCookie != null)
            {
                int.TryParse(couponPercentCookie, out couponPercent);
            }

            // Tính tổng tiền sau khi áp dụng giảm giá
            var grandTotal = cartItems.Sum(x => x.Price * x.Quantity);
            var discountAmount = grandTotal * couponPercent / 100;
            var finalTotal = grandTotal - discountAmount + shippingPrice;

            CartItemViewModel cartItemViewModel = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(x => x.Price * x.Quantity),
                ShippingPrice = shippingPrice,
                CouponCode = coupon_code
            };

            ViewBag.CouponPercent = couponPercent;
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.FinalTotal = finalTotal;

            return View(cartItemViewModel);
        }

        public async Task<IActionResult> Add(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItems = cart.Where(x => x.ProductId == Id).FirstOrDefault();
            if (cartItems == null)
            {
                cart.Add(new CartItemModel(product));
            }
            else
            {
                cartItems.Quantity++;
            }
            HttpContext.Session.SetJson("Cart", cart);

            TempData["success"] = "Add Item to Cart Successfully";

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Increase(int Id, string size)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItem = cart.FirstOrDefault(c => c.ProductId == Id && c.Size == size);

            if (cartItem == null)
            {
                TempData["error"] = "Sản phẩm không tồn tại trong giỏ hàng.";
                return RedirectToAction("Index");
            }

            // Lấy thông tin số lượng tối đa từ ProductSize
            var productSize = await _dataContext.ProductSizes
                .FirstOrDefaultAsync(ps => ps.ProductId == Id && ps.Size == size);

            if (productSize == null)
            {
                TempData["error"] = "Không tìm thấy thông tin kích thước sản phẩm.";
                return RedirectToAction("Index");
            }

            if (cartItem.Quantity < productSize.Quantity)
            {
                cartItem.Quantity++;
                TempData["success"] = "Tăng số lượng sản phẩm thành công!";
            }
            else
            {
                TempData["error"] = $"Số lượng tối đa cho sản phẩm này (size {size}) là {productSize.Quantity}.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetJson("Cart", cart);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Decrease(int Id, string size)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItems = cart.FirstOrDefault(x => x.ProductId == Id && x.Size == size);
            if (cartItems == null)
            {
                TempData["error"] = "Sản phẩm không tồn tại trong giỏ hàng.";
                return RedirectToAction("Index");
            }

            if (cartItems.Quantity > 1)
            {
                cartItems.Quantity--;
            }
            else
            {
                cart.Remove(cartItems);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }

            TempData["success"] = "Giảm số lượng sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int Id, string size)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            var cartItem = cart.FirstOrDefault(x => x.ProductId == Id && x.Size == size);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }

            TempData["success"] = "Xóa sản phẩm khỏi giỏ hàng thành công!";
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            TempData["success"] = "Xóa toàn bộ giỏ hàng thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Cart/GetShipping")]
        public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
        {
            var existingShipping = await _dataContext.Shippings
                .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

            decimal shippingPrice = 0;

            if (existingShipping != null)
            {
                shippingPrice = existingShipping.Price;
            }
            else
            {
                shippingPrice = 50000;
            }
            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);
            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true
                };

                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
            }
            return Json(new { shippingPrice });
        }

        [HttpPost]
        [Route("Cart/GetCoupon")]
        public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
        {
            var validCoupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(x => x.Code == coupon_value && x.Quantity >= 1);

            if (validCoupon == null)
            {
                return Ok(new { success = false, message = "Coupon không tồn tại." });
            }

            if (validCoupon.Status != 1)
            {
                return Ok(new { success = false, message = "Coupon không hoạt động." });
            }

            TimeSpan remainingTime = validCoupon.EndDate - DateTime.Now;
            int daysRemaining = remainingTime.Days;

            if (daysRemaining < 0)
            {
                return Ok(new { success = false, message = "Coupon đã hết hạn." });
            }

            string couponTitle = validCoupon.Code + " | " + validCoupon.Description;

            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
                Response.Cookies.Append("CouponPercent", validCoupon.Percent.ToString(), cookieOptions);
                return Ok(new { success = true, message = "Áp dụng coupon thành công!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding apply coupon cookie: {ex.Message}");
                return Ok(new { success = false, message = "Áp dụng coupon thất bại." });
            }
        }

        [HttpPost]
        [Route("Cart/RemoveShippingCookie")]
        public IActionResult RemoveShippingCookie()
        {
            Response.Cookies.Delete("ShippingPrice");
            return Json(new { success = true });
        }

        [HttpPost]
        [Route("Cart/RemoveCouponCookie")]
        public IActionResult RemoveCouponCookie()
        {
            Response.Cookies.Delete("CouponTitle");
            Response.Cookies.Delete("CouponPercent");
            return Json(new { success = true });
        }
    }
}