using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSellingShoes.Models;
using WebSellingShoes.Models.ViewModels;
using WebSellingShoes.Repository;

namespace WebSellingShoes.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        public ProductController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        /*public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10; // 10 sản phẩm mỗi trang
            if (pg < 1)
            {
                pg = 1; // Đảm bảo trang không nhỏ hơn 1
            }

            // Lấy tổng số sản phẩm từ database
            int recsCount = await _dataContext.Products.CountAsync(); // Tổng số sản phẩm

            // Tạo đối tượng Paginate
            var pager = new Paginate(recsCount, pg, pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var products = await _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderBy(p => p.Id) // Sắp xếp để đảm bảo thứ tự nhất quán
                .Skip((pg - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền đối tượng Paginate vào ViewBag
            ViewBag.Pager = pager;

            return View(products);
        }*/

        [Route("product/details/{Id}")]
        [HttpGet]
        public async Task<IActionResult> Details(int Id)
        {
            var product = await _dataContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductSizes)
                .Include(p => p.ProductImages) // Đảm bảo dòng này có mặt
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToListAsync();

            var model = new ProductDetailsViewModel
            {
                ProductDetails = product,
                ProductSizes = product.ProductSizes,
                Ratings = product.Ratings
            };

            ViewBag.RelatedProducts = relatedProducts;
            return View(model);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .ToListAsync();

            ViewBag.Keyword = searchTerm;

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentProduct(RatingModel ratingModel)
        {
            if (ModelState.IsValid)
            {
                var existingRating = await _dataContext.Ratings
                    .FirstOrDefaultAsync(r => r.ProductId == ratingModel.ProductId && r.Email == ratingModel.Email);

                if (existingRating != null)
                {
                    existingRating.Star = ratingModel.Star;
                    existingRating.Comment = ratingModel.Comment;
                    existingRating.CreateDate = DateTime.Now;
                    _dataContext.Ratings.Update(existingRating);
                }
                else
                {
                    var ratingEntity = new RatingModel
                    {
                        ProductId = ratingModel.ProductId,
                        UserName = ratingModel.UserName,
                        Email = ratingModel.Email,
                        Comment = ratingModel.Comment,
                        Star = ratingModel.Star,
                        CreateDate = DateTime.Now
                    };

                    _dataContext.Ratings.Add(ratingEntity);
                }

                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm đánh giá thành công";

                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);

                return RedirectToAction("Details", new { id = ratingModel.ProductId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, string size, int quantity)
        {
            var productSize = await _dataContext.ProductSizes
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.ProductId == id && ps.Size == size);

            if (productSize == null || productSize.Quantity < quantity)
            {
                TempData["error"] = "Số lượng không đủ.";
                return RedirectToAction("Details", new { id });
            }

            // Thêm sản phẩm vào giỏ hàng
            var cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == id && c.Size == size);

            if (cartItem == null)
            {
                cart.Add(new CartItemModel
                {
                    ProductId = id,
                    ProductName = productSize.Product.Name, // Lưu tên sản phẩm                    
                    Image = productSize.Product.Image, // Lưu hình ảnh
                    Size = size,
                    Quantity = quantity,
                    Price = productSize.Product.Price
                });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            HttpContext.Session.SetJson("Cart", cart);

            TempData["success"] = "Thêm vào giỏ hàng thành công.";
            return RedirectToAction("Details", new { id });
        }
    }
}