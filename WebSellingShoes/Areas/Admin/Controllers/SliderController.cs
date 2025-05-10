using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;

namespace WebSellingShoes.Areas.Admin.Controllers
{
    [Area("admin")]
    //[Route("admin/[controller]/[action]")]
    [Route("admin/[controller]/")]
    [Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webhostEnvironment;

        public SliderController(DataContext context, IWebHostEnvironment webhostEnvironment)
        {
            _dataContext = context;
            _webhostEnvironment = webhostEnvironment;

        }

        [Route("index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Sliders.OrderByDescending(x => x.Id).ToListAsync());
        }

        [Route("create")]
        [HttpGet]
        public IActionResult Create()
        {           
            return View();
        }

        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel slider)
        {
            if (ModelState.IsValid)
            {
                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webhostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    slider.Image = imageName;
                }

                _dataContext.Add(slider);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm slide thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Thêm slide không thành công";
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
            return View(slider);
        }

        [Route("edit/{Id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);            
            return View(slider);
        }

        [Route("edit/{Id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SliderModel slider)
        {
            var oldSlider = _dataContext.Sliders.Find(slider.Id); // lay ra san pham cu de lay ten anh cu de xoa anh cu theo id

            if (ModelState.IsValid)
            {               
                if (slider.ImageUpload != null)
                {
                    // luu anh moi
                    string uploadsDir = Path.Combine(_webhostEnvironment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    // xoa anh cu
                    string oldFilePath = Path.Combine(uploadsDir, oldSlider.Image);

                    try
                    {
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Có lỗi khi xóa ảnh cũ");
                    }

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    oldSlider.Image = imageName;

                }

                // cap nhat lai anh moi
                oldSlider.Name = slider.Name;
                oldSlider.Description = slider.Description;
                oldSlider.Status = slider.Status;


                _dataContext.Update(oldSlider); // cap nhat lai san pham cu
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật slide thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Cập nhật slide không thành công";
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
            return View(slider);
        }
    }
}
