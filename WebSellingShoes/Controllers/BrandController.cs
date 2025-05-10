using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;

namespace WebSellingShoes.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;

        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            // lấy ra brand theo slug
            BrandModel brand = _dataContext.Brands.Where(x => x.Slug == Slug).FirstOrDefault();
            if (brand == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Slug = Slug;
            // lấy ra sản phẩm thuộc category đó theo id
            IQueryable<ProductModel> productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);
            var count = await productsByBrand.CountAsync();
            if (count > 0)
            {
                // Apply sorting based on sort_by parameter
                if (sort_by == "price_increase")
                {
                    productsByBrand = productsByBrand.OrderBy(p => p.Price);
                }
                else if (sort_by == "price_decrease")
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Price);
                }
                else if (sort_by == "price_newest")
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                }
                else if (sort_by == "price_oldest")
                {
                    productsByBrand = productsByBrand.OrderBy(p => p.Id);
                }
                else if (startprice != "" && endprice != "")
                {
                    decimal startPriceValue;
                    decimal endPriceValue;

                    if (decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endprice, out endPriceValue))
                    {
                        productsByBrand = productsByBrand.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                    }
                    else
                    {
                        productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                    }
                }
                else
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                }

                decimal minPrice = await productsByBrand.MinAsync(p => p.Price);
                decimal maxPrice = await productsByBrand.MaxAsync(p => p.Price);


                ViewBag.sort_key = sort_by;

                ViewBag.count = count;


                ViewBag.minprice = minPrice;
                ViewBag.maxprice = maxPrice;
            }

            return View(await productsByBrand.ToListAsync());// async phải có await phương thức trả về Task mức bất đông bộ
            // desc theo id để hiển thị sản phẩm mới nhất lên đầu, tolisstasync để chuyển đổi dữ liệu sang dạng list
        }
    }
}
