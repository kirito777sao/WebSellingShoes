using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;

namespace WebSellingShoes.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/product")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webhostEnvironment;

        public ProductController(DataContext context, IWebHostEnvironment webhostEnvironment)
        {
            _dataContext = context;
            _webhostEnvironment = webhostEnvironment;
        }

        [Route("index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products
                .OrderByDescending(x => x.Id)
                .Include(x => x.Category)
                .Include(x => x.Brand)
                .Include(x => x.ProductSizes)
                .Include(x => x.ProductImages)
                .ToListAsync());
        }

        [Route("create")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product, IFormFile[] secondaryImagesUpload)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-").ToLower();
                var slug = await _dataContext.Products.FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Tên sản phẩm đã tồn tại");
                    return View(product);
                }

                // Xử lý ảnh chính
                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webhostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fs);
                    }
                    product.Image = imageName;
                }

                // Xử lý ảnh phụ
                if (secondaryImagesUpload != null && secondaryImagesUpload.Length > 0)
                {
                    string uploadsDir = Path.Combine(_webhostEnvironment.WebRootPath, "media/products");
                    foreach (var file in secondaryImagesUpload.Take(5)) // Giới hạn 5 ảnh
                    {
                        if (file != null && file.Length > 0)
                        {
                            string imageName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsDir, imageName);
                            using (var fs = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fs);
                            }
                            product.ProductImages.Add(new ProductImageModel { ImageName = imageName });
                        }
                    }
                }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Thêm sản phẩm không thành công";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n ", errors);
                return BadRequest(errorMessage);
            }
        }

        [Route("edit/{Id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            ProductModel product = await _dataContext.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == Id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }

        [Route("edit/{Id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductModel product, IFormFile[] secondaryImagesUpload, int[] deletedImageIds)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            var oldProduct = await _dataContext.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == product.Id);
            if (oldProduct == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-").ToLower();
                var slug = await _dataContext.Products.FirstOrDefaultAsync(x => x.Slug == product.Slug && x.Id != product.Id);
                if (slug != null)
                {
                    ModelState.AddModelError("Name", "Tên sản phẩm đã tồn tại");
                    return View(product);
                }

                // Xử lý ảnh chính
                string uploadsDir = Path.Combine(_webhostEnvironment.WebRootPath, "media/products");
                if (product.ImageUpload != null)
                {
                    // Xóa ảnh chính cũ
                    if (!string.IsNullOrEmpty(oldProduct.Image))
                    {
                        string oldFilePath = Path.Combine(uploadsDir, oldProduct.Image);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu ảnh chính mới
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fs);
                    }
                    oldProduct.Image = imageName;
                }

                // Xử lý xóa ảnh phụ
                if (deletedImageIds != null && deletedImageIds.Length > 0)
                {
                    var imagesToDelete = oldProduct.ProductImages.Where(pi => deletedImageIds.Contains(pi.Id)).ToList();
                    foreach (var image in imagesToDelete)
                    {
                        string filePath = Path.Combine(uploadsDir, image.ImageName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        oldProduct.ProductImages.Remove(image);
                    }
                }

                // Xử lý thêm ảnh phụ mới
                if (secondaryImagesUpload != null && secondaryImagesUpload.Length > 0)
                {
                    foreach (var file in secondaryImagesUpload.Take(5 - oldProduct.ProductImages.Count))
                    {
                        if (file != null && file.Length > 0)
                        {
                            string imageName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsDir, imageName);
                            using (var fs = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fs);
                            }
                            oldProduct.ProductImages.Add(new ProductImageModel { ImageName = imageName });
                        }
                    }
                }

                // Cập nhật các trường khác
                oldProduct.Name = product.Name;
                oldProduct.Description = product.Description;
                oldProduct.Price = product.Price;
                oldProduct.CapitalPrice = product.CapitalPrice;
                oldProduct.CategoryId = product.CategoryId;
                oldProduct.BrandId = product.BrandId;

                _dataContext.Update(oldProduct);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Cập nhật sản phẩm không thành công";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n ", errors);
                return BadRequest(errorMessage);
            }
        }

        [Route("delete/{Id}")]
        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
            ProductModel product = await _dataContext.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == Id);
            if (product == null)
            {
                TempData["error"] = "Xóa sản phẩm không thành công";
                return NotFound();
            }

            string uploadsDir = Path.Combine(_webhostEnvironment.WebRootPath, "media/products");

            // Xóa ảnh chính
            if (!string.IsNullOrEmpty(product.Image))
            {
                string oldFilePath = Path.Combine(uploadsDir, product.Image);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Xóa ảnh phụ
            foreach (var image in product.ProductImages)
            {
                string filePath = Path.Combine(uploadsDir, image.ImageName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa sản phẩm thành công";
            return RedirectToAction("Index");
        }

        [Route("AddQuantity")]
        [HttpGet]
        public async Task<IActionResult> AddQuantity(int id)
        {
            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Id = id;
            ViewBag.ProductByQuantity = await _dataContext.ProductSizes
                .Where(ps => ps.ProductId == id)
                .ToListAsync();

            return View(new ProductSizeModel { ProductId = id });
        }

        [Route("UpdateMoreQuantity")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMoreQuantity(ProductSizeModel model)
        {
            if (ModelState.IsValid)
            {
                var productSize = await _dataContext.ProductSizes
                    .FirstOrDefaultAsync(ps => ps.ProductId == model.ProductId && ps.Size == model.Size);

                if (productSize != null)
                {
                    productSize.Quantity += model.Quantity;
                    productSize.DateCreated = DateTime.Now;
                    _dataContext.Update(productSize);
                }
                else
                {
                    model.DateCreated = DateTime.Now;
                    _dataContext.Add(model);
                }

                var product = await _dataContext.Products.FindAsync(model.ProductId);
                product.Quantity += model.Quantity;
                _dataContext.Update(product);

                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật số lượng sản phẩm thành công.";
                return RedirectToAction("Index");
            }

            ViewBag.Id = model.ProductId;
            ViewBag.ProductByQuantity = await _dataContext.ProductSizes
                .Where(ps => ps.ProductId == model.ProductId)
                .ToListAsync();

            return View("AddQuantity", model);
        }
    }
}