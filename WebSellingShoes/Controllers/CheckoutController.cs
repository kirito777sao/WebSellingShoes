using System.Security.Claims;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebSellingShoes.Areas.Admin.Repository;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;
using WebSellingShoes.Services.Momo;
using WebSellingShoes.Services.Vnpay;

namespace WebSellingShoes.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IVnPayService _vnPayService;
        private readonly IMomoService _momoService;
        private readonly ILogger<CheckoutController> _logger;
        public CheckoutController(ILogger<CheckoutController> logger, DataContext context, IEmailSender emailSender, IVnPayService vnPayService,  IMomoService momoService)//(DataContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
            _vnPayService = vnPayService;
            _momoService = momoService;
            _logger = logger;
        }

        /*public async Task<IActionResult> Checkout(string paymentMethod, string paymentId)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email).Value;
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var ordercode = Guid.NewGuid().ToString();
                // Nhận shipping giá từ cookie
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;
                var orderItem = new OrderModel();
                orderItem.OrderCode = ordercode;

                //Nhận Coupon code từ cookie
                var coupon_code = Request.Cookies["CouponTitle"];

                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }
                else
                {
                    shippingPrice = 0;
                }
                orderItem.ShippingCost = shippingPrice;
                orderItem.CouponCode = coupon_code;
                orderItem.UserName = userEmail;
                if (paymentMethod != "MOMO" || paymentMethod != "VnPay")
                {
                    orderItem.PaymentMethod = "COD";
                }
                else if (paymentMethod == "VnPay")
                {
                    orderItem.PaymentMethod = "VnPay" + " | " + paymentId;
                }
                else
                {
                    orderItem.PaymentMethod = "Momo" + " | " + paymentMethod;
                }
                orderItem.Status = 1;
                orderItem.CreateDate = DateTime.Now;

                _context.Add(orderItem);
                _context.SaveChanges();

                //add order details
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var item in cartItems)
                {
                    var orderDetails = new OrderDetail
                    {
                        UserName = userEmail,
                        OrderCode = ordercode,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    //update product quantity
                    var product = await _context.Products.Where(p => p.Id == item.ProductId).FirstAsync();
                    product.Quantity -= item.Quantity;
                    product.Sold += item.Quantity;

                    _context.Update(product);

                    _context.Add(orderDetails);
                    _context.SaveChanges();
                }

                HttpContext.Session.Remove("Cart");
                // send email
                var receiver = userEmail;//"duchoan772003@gmail.com";
                var subject = "Đặt hàng thành công.";
                var message = "Đơn hàng xử lý thành công, trải nghiệm dịch vụ nhé.";

                await _emailSender.SendEmailAsync(receiver, subject, message);

                TempData["success"] = "Tạo đơn hàng thành công, vui lòng chờ duyệt đơn";
                return RedirectToAction("Index", "Cart");
            }
            return View();
        }*/

        public async Task<IActionResult> Checkout(string paymentMethod, string paymentId)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                var ordercode = Guid.NewGuid().ToString();
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;
                var orderItem = new OrderModel { OrderCode = ordercode };
                var couponCode = Request.Cookies["CouponTitle"];
                decimal discountAmount = 0;

                if (!string.IsNullOrEmpty(shippingPriceCookie))
                {
                    try
                    {
                        shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
                    }
                    catch (Exception)
                    {
                        if (decimal.TryParse(shippingPriceCookie, out decimal parsedShippingPrice))
                        {
                            shippingPrice = parsedShippingPrice;
                        }
                    }
                }

                if (shippingPrice == 0)
                {
                    shippingPrice = 50000m;
                    Response.Cookies.Append("ShippingPrice", JsonConvert.SerializeObject(shippingPrice), new CookieOptions
                    {
                        Expires = DateTime.Now.AddHours(1),
                        Path = "/",
                        HttpOnly = true
                    });
                }

                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                if (!cartItems.Any())
                {
                    TempData["error"] = "Giỏ hàng trống. Không thể tạo đơn hàng.";
                    return RedirectToAction("Index", "Cart");
                }

                decimal totalPrice = cartItems.Sum(item => item.Price * item.Quantity);

                if (!string.IsNullOrEmpty(couponCode))
                {
                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
                    if (coupon != null)
                    {
                        discountAmount = totalPrice * coupon.Percent / 100;
                    }
                }

                decimal finalPrice = totalPrice - discountAmount + shippingPrice;

                orderItem.ShippingCost = shippingPrice;
                orderItem.CouponCode = couponCode;
                orderItem.UserName = userEmail;
                orderItem.TotalPrice = finalPrice;
                orderItem.PaymentMethod = paymentMethod switch
                {
                    "VnPay" => $"VnPay | {paymentId}",
                    "Momo" => $"Momo | {paymentId}",
                    _ => "COD"
                };
                orderItem.Status = 1;
                orderItem.CreateDate = DateTime.Now;

                _context.Add(orderItem);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    var productSize = await _context.ProductSizes
                        .FirstOrDefaultAsync(ps => ps.ProductId == item.ProductId && ps.Size == item.Size);
                    if (productSize == null || productSize.Quantity < item.Quantity)
                    {
                        TempData["error"] = $"Sản phẩm {item.ProductId} (kích cỡ {item.Size}) không đủ số lượng.";
                        return RedirectToAction("Index", "Cart");
                    }
                    productSize.Quantity -= item.Quantity;

                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        TempData["error"] = $"Sản phẩm {item.ProductId} không tồn tại.";
                        return RedirectToAction("Index", "Cart");
                    }
                    product.Quantity -= item.Quantity;
                    product.Sold += item.Quantity;

                    var orderDetails = new OrderDetail
                    {
                        UserName = userEmail,
                        OrderCode = ordercode,
                        ProductId = item.ProductId,
                        Size = item.Size,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };

                    _context.Update(productSize);
                    _context.Update(product);
                    _context.Add(orderDetails);
                }

                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("Cart");

                var receiver = userEmail;
                var subject = "Đặt hàng thành công";
                var message = $"Đơn hàng {ordercode} đã được tạo thành công. Tổng giá trị: {finalPrice.ToString("#,##0 VNĐ")}. Vui lòng chờ duyệt đơn.";
                await _emailSender.SendEmailAsync(receiver, subject, message);
                

                TempData["success"] = "Tạo đơn hàng thành công, vui lòng chờ duyệt đơn.";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception)
            {
                TempData["error"] = "Đã xảy ra lỗi khi tạo đơn hàng. Vui lòng thử lại.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email).Value;
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }

            var coupon_code = Request.Cookies["CouponTitle"];
            decimal discountAmount = 0;

            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            decimal totalPrice = cartItems.Sum(item => item.Price * item.Quantity);

            if (!string.IsNullOrEmpty(coupon_code))
            {
                var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == coupon_code);
                if (coupon != null)
                {
                    discountAmount = totalPrice * coupon.Percent / 100;
                }
            }

            decimal finalPrice = totalPrice - discountAmount + shippingPrice;

            model.Amount = (double)finalPrice;

            var response = await _momoService.CreatePaymentMomo(model);
            return Redirect(response.QrCodeUrl);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy phí vận chuyển từ cookie
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;
                if (!string.IsNullOrEmpty(shippingPriceCookie))
                {
                    try
                    {
                        shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
                    }
                    catch (Exception)
                    {
                        if (decimal.TryParse(shippingPriceCookie, out decimal parsedShippingPrice))
                        {
                            shippingPrice = parsedShippingPrice;
                        }
                    }
                }

                // Nếu shippingPrice vẫn là 0, đặt giá trị mặc định và lưu cookie
                if (shippingPrice == 0)
                {
                    shippingPrice = 50000m; // Dựa trên hình ảnh
                    Response.Cookies.Append("ShippingPrice", JsonConvert.SerializeObject(shippingPrice), new CookieOptions
                    {
                        Expires = DateTime.Now.AddHours(1),
                        Path = "/",
                        HttpOnly = true
                    });
                }

                // Lấy mã giảm giá từ cookie
                var couponCode = Request.Cookies["CouponTitle"];
                decimal discountAmount = 0;

                // Lấy giỏ hàng từ session
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                if (!cartItems.Any())
                {
                    TempData["error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
                    return RedirectToAction("Index", "Cart");
                }

                // Tính tổng giá sản phẩm
                decimal totalPrice = cartItems.Sum(item => item.Price * item.Quantity);

                // Áp dụng mã giảm giá nếu có
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
                    if (coupon != null)
                    {
                        discountAmount = totalPrice * coupon.Percent / 100;
                    }
                }

                // Tính tổng thành tiền
                decimal finalPrice = totalPrice - discountAmount + shippingPrice;

                // Gán giá trị vào model
                model.Amount = (double)finalPrice;
                model.OrderDescription = $"Thanh toán đơn hàng cho {userEmail}";
                model.Name = userEmail;
                model.OrderType = "250000";

                // Gọi VnPayService để tạo URL thanh toán
                var paymentUrl = _vnPayService.CreatePaymentUrl(model, HttpContext);
                if (string.IsNullOrEmpty(paymentUrl))
                {
                    TempData["error"] = "Không thể tạo URL thanh toán VnPay. Vui lòng thử lại.";
                    return RedirectToAction("Index", "Cart");
                }

                return Redirect(paymentUrl);
            }
            catch (Exception)
            {
                TempData["error"] = "Đã xảy ra lỗi khi tạo URL thanh toán. Vui lòng thử lại.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            try
            {
                var response = _vnPayService.PaymentExecute(Request.Query);
                if (response == null)
                {
                    TempData["error"] = "Không nhận được phản hồi từ VnPay.";
                    return RedirectToAction("Index", "Cart");
                }

                var vnpAmountStr = Request.Query["vnp_Amount"];
                decimal finalPrice = string.IsNullOrEmpty(vnpAmountStr) ? 0 : decimal.Parse(vnpAmountStr) / 100;

                if (response.VnPayResponseCode == "00")
                {
                    var newVnpayInsert = new VnpayModel
                    {
                        PaymentMethod = response.PaymentMethod,
                        OrderDescription = response.OrderDescription,
                        TransactionId = response.TransactionId,
                        PaymentId = response.PaymentId,
                        DateCreated = DateTime.Now
                    };

                    _context.Add(newVnpayInsert);
                    await _context.SaveChangesAsync();

                    await Checkout("VnPay", response.PaymentId);

                    ViewData["FinalPrice"] = finalPrice;
                    return View(response);
                }
                else
                {
                    TempData["error"] = $"Giao dịch VnPay thất bại. Mã lỗi: {response.VnPayResponseCode}";
                    return RedirectToAction("Index", "Cart");
                }
            }
            catch (Exception)
            {
                TempData["error"] = "Đã xảy ra lỗi khi xử lý thanh toán VnPay.";
                return RedirectToAction("Index", "Cart");
            }
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCallBackMomo(MomoInfoModel model)
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;
            if (requestQuery["resultCode"] != 0)
            {
                var newMomoInsert = new MomoInfoModel
                {
                    OrderId = requestQuery["orderId"],
                    FullName = User.FindFirstValue(ClaimTypes.Email),
                    Amount = decimal.Parse(requestQuery["Amount"]),
                    OrderInfo = requestQuery["orderInfo"],
                    DatePaid = DateTime.Now
                };
                _context.Add(newMomoInsert);
                await _context.SaveChangesAsync();
                // tien hanh dat don hang khi thanh toan vnpay thanh cong
                var paymentMethod = response.OrderInfo;
                var paymentId = response.OrderId;
                await Checkout(paymentMethod, paymentId);
            }
            else
            {
                TempData["success"] = "Dã hůy giao dich Momo.";
                return RedirectToAction("Index", "Cart");               
            }
            // Call checkout method after saving MomoInfo
            //await Checkout(requestQuery["orderId"]);
            return View(response);
        }

        /*public async Task<IActionResult> Checkout(string paymentMethod, string paymentId)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email).Value;
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var ordercode = Guid.NewGuid().ToString();
                // Nhận shipping giá từ cookie
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;
                var orderItem = new OrderModel();
                orderItem.OrderCode = ordercode;

                //Nhận Coupon code từ cookie
                var coupon_code = Request.Cookies["CouponTitle"];
                decimal discountAmount = 0;

                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }
                else
                {
                    shippingPrice = 0;
                }

                // Tính tổng giá trị đơn hàng
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                decimal totalPrice = cartItems.Sum(item => item.Price * item.Quantity);

                // Áp dụng mã giảm giá nếu có
                if (!string.IsNullOrEmpty(coupon_code))
                {
                    var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == coupon_code);
                    if (coupon != null)
                    {
                        discountAmount = totalPrice * coupon.Percent / 100;
                    }
                }

                // Tính tổng giá trị sau khi áp dụng mã giảm giá và phí ship
                decimal finalPrice = totalPrice - discountAmount + shippingPrice;

                orderItem.ShippingCost = shippingPrice;
                orderItem.CouponCode = coupon_code;
                orderItem.UserName = userEmail;
                orderItem.TotalPrice = finalPrice; // Thêm trường TotalPrice vào OrderModel
                if (paymentMethod != "MOMO" || paymentMethod != "VnPay")
                {
                    orderItem.PaymentMethod = "COD";
                }
                else if (paymentMethod == "VnPay")
                {
                    orderItem.PaymentMethod = "VnPay" + " | " + paymentId;
                }
                else
                {
                    orderItem.PaymentMethod = "Momo" + " | " + paymentMethod;
                }
                orderItem.Status = 1;
                orderItem.CreateDate = DateTime.Now;

                _context.Add(orderItem);
                _context.SaveChanges();

                //add order details
                foreach (var item in cartItems)
                {
                    var orderDetails = new OrderDetail
                    {
                        UserName = userEmail,
                        OrderCode = ordercode,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    //update product quantity
                    var product = await _context.Products.Where(p => p.Id == item.ProductId).FirstAsync();
                    product.Quantity -= item.Quantity;
                    product.Sold += item.Quantity;

                    _context.Update(product);

                    _context.Add(orderDetails);
                    _context.SaveChanges();
                }

                HttpContext.Session.Remove("Cart");
                // send email
                var receiver = userEmail;//"duchoan772003@gmail.com";
                var subject = "Đặt hàng thành công.";
                var message = "Đơn hàng xử lý thành công, trải nghiệm dịch vụ nhé.";

                await _emailSender.SendEmailAsync(receiver, subject, message);

                TempData["success"] = "Tạo đơn hàng thành công, vui lòng chờ duyệt đơn";
                return RedirectToAction("Index", "Cart");
            }
            return View();
        }*/
    }
}
