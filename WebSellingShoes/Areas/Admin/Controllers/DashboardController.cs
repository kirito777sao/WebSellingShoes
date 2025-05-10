using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;

namespace WebSellingShoes.Areas.Admin.Controllers
{
    
    [Area("admin")]
    [Route("Admin/Dashboard")]
    [Authorize(Roles = "Seller,Admin")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;

        public DashboardController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;
            return View();
        }

        [HttpPost]
        [Route("GetChartData")]
        public IActionResult GetChartData()
        {
            var chartData = from o in _dataContext.Orders
                            join od in _dataContext.OrderDetails on o.OrderCode equals od.OrderCode
                            group new { o, od } by o.CreateDate.Date into g
                            orderby g.Key
                            select new
                            {
                                date = g.Key.ToString("yyyy-MM-dd"),
                                revenue = g.Sum(x => x.od.Quantity * x.od.Price),
                                quantity = g.Sum(x => x.od.Quantity)
                            };

            var result = chartData.ToList();
            Debug.WriteLine($"GetChartData: Returned {result.Count} records");
            return Json(result);
        }

        [HttpPost]
        [Route("GetChartDataBySelect")]
        public IActionResult GetChartDataBySelect([FromBody] DateRangeRequest request)
        {
            if (request == null || !request.Days.HasValue)
            {
                Debug.WriteLine("GetChartDataBySelect: Invalid or missing days");
                return Json(new List<object>());
            }

            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-request.Days.Value + 1); // Include end date

            var chartData = from o in _dataContext.Orders
                            join od in _dataContext.OrderDetails on o.OrderCode equals od.OrderCode
                            where o.CreateDate.Date >= startDate && o.CreateDate.Date <= endDate
                            group new { o, od } by o.CreateDate.Date into g
                            orderby g.Key
                            select new
                            {
                                date = g.Key.ToString("yyyy-MM-dd"),
                                revenue = g.Sum(x => x.od.Quantity * x.od.Price),
                                quantity = g.Sum(x => x.od.Quantity)
                            };

            var result = chartData.ToList();
            Debug.WriteLine($"GetChartDataBySelect: Days={request.Days}, Start={startDate:yyyy-MM-dd}, End={endDate:yyyy-MM-dd}, Returned {result.Count} records");
            return Json(result);
        }

        [HttpPost]
        [Route("FilterData")]
        public IActionResult FilterData([FromBody] DateRangeRequest request)
        {
            if (request == null || !request.StartDate.HasValue || !request.EndDate.HasValue)
            {
                Debug.WriteLine("FilterData: Invalid or missing date range");
                return Json(new List<object>());
            }

            var startDate = request.StartDate.Value.Date;
            var endDate = request.EndDate.Value.Date;

            var chartData = from o in _dataContext.Orders
                            join od in _dataContext.OrderDetails on o.OrderCode equals od.OrderCode
                            where o.CreateDate.Date >= startDate && o.CreateDate.Date <= endDate
                            group new { o, od } by o.CreateDate.Date into g
                            orderby g.Key
                            select new
                            {
                                date = g.Key.ToString("yyyy-MM-dd"),
                                revenue = g.Sum(x => x.od.Quantity * x.od.Price),
                                quantity = g.Sum(x => x.od.Quantity)
                            };

            var result = chartData.ToList();
            Debug.WriteLine($"FilterData: Start={startDate:yyyy-MM-dd}, End={endDate:yyyy-MM-dd}, Returned {result.Count} records");
            return Json(result);
        }
    }

    public class DateRangeRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Days { get; set; }
    }
}
