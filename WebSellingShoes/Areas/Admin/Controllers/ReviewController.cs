using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;

namespace WebSellingShoes.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class ReviewController : Controller
    {
        private readonly DataContext _dataContext;
        public ReviewController(DataContext context)
        {
            _dataContext = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reviews = await _dataContext.Ratings.Include(r => r.Product).ToListAsync();
            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _dataContext.Ratings.Include(r => r.Product).FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
            {
                return NotFound();
            }
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RatingModel review)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _dataContext.Update(review);
                    await _dataContext.SaveChangesAsync();
                    TempData["success"] = "Review updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _dataContext.Ratings.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _dataContext.Ratings.Remove(review);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Review deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _dataContext.Ratings.Any(e => e.Id == id);
        }
    }
}
