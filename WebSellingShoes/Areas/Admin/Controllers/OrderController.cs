using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;
using WebSellingShoes.Repository;

namespace WebSellingShoes.Controllers
{
    [Area("admin")]
    [Route("admin/order")]
    [Authorize(Roles = "Seller,Admin")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        public OrderController(DataContext context)
        {
            _dataContext = context;
        }
        [HttpGet]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }

        [HttpGet]
        [Route("vieworder/{ordercode}")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode).ToListAsync();

            var Order = _dataContext.Orders.Where(o => o.OrderCode == ordercode).First();

            // Retrieve the coupon percentage from the cookie
            var couponPercent = 0;
            if (Request.Cookies.TryGetValue("CouponPercent", out var couponPercentStr))
            {
                int.TryParse(couponPercentStr, out couponPercent);
            }

            // Calculate the total price
            var totalPrice = DetailsOrder.Sum(od => od.Price * od.Quantity);
            var discountAmount = totalPrice * couponPercent / 100;
            var discountedTotalPrice = totalPrice - discountAmount + Order.ShippingCost;

            ViewBag.ShippingCost = Order.ShippingCost;
            ViewBag.CouponCode = Order.CouponCode;
            ViewBag.Status = Order.Status;

            ViewBag.TotalPrice = totalPrice;
            ViewBag.DiscountedTotalPrice = discountedTotalPrice;
            ViewBag.CouponPercent = couponPercent;
            ViewBag.DiscountAmount = discountAmount;

            return View(DetailsOrder);
        }
        [HttpPost]
        [Route("updateorder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _dataContext.Update(order);

            if (status == 0)
            {
                // lay du lieu order detail dua vao order.OrderCode
                var DetailsOrder = await _dataContext.OrderDetails
                    .Include(od => od.Product)
                    .Where(od => od.OrderCode == od.OrderCode)
                    .Select(od => new
                    {
                        od.Quantity,
                        od.Product.Price,
                        od.Product.CapitalPrice
                    }).ToListAsync();

                //lay data thong ke dua vao ngay dat hang
                var statisticalModel = await _dataContext.Statisticals
                    .FirstOrDefaultAsync(s => s.DateCreated.Date == order.CreateDate.Date);
                if (statisticalModel != null)
                {
                    foreach(var orderDetail in DetailsOrder)
                    {
                        // ton tai ngay thi cong don
                        statisticalModel.Quantity += 1;
                        statisticalModel.Sold += orderDetail.Quantity;
                        statisticalModel.Revenue += orderDetail.Quantity * orderDetail.Price;
                        statisticalModel.Profit += orderDetail.Price - orderDetail.CapitalPrice;
                    }
                    _dataContext.Update(statisticalModel);
                }
                else
                {
                    int new_quantity = 0;
                    int new_sold = 0;
                    decimal new_profit = 0;
                    foreach(var orderDetail in DetailsOrder) {
                        new_quantity += 1;
                        new_sold += orderDetail.Quantity;
                        new_profit += orderDetail.Price - orderDetail.CapitalPrice;

                        statisticalModel = new StatisticalModel
                        {
                            DateCreated = order.CreateDate,
                            Quantity = new_quantity,
                            Sold = new_sold,
                            Revenue = orderDetail.Quantity * orderDetail.Price,
                            Profit = new_profit
                        };
                    }
                    _dataContext.Add(statisticalModel);
                }
            }

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }
        [HttpGet]
        [Route("delete")]
        public async Task<IActionResult> Delete(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }
            try
            {
                // Delete order details
                var orderDetails = _dataContext.OrderDetails.Where(od => od.OrderCode == ordercode);
                _dataContext.OrderDetails.RemoveRange(orderDetails);

                // Delete order
                _dataContext.Orders.Remove(order);

                await _dataContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }
    }
}
