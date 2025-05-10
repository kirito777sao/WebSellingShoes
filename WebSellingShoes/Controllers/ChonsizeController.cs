using Microsoft.AspNetCore.Mvc;

namespace WebSellingShoes.Controllers
{
    public class ChonsizeController : Controller
    {
        public IActionResult SizeGuide()
        {
            // Trả về view mà không áp dụng layout chính
            return View("~/Views/Shared/_Chonsize.cshtml");
        }
    }
}
