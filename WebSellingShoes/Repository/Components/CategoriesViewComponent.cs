using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebSellingShoes.Repository.Components
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext; // lấy ra csdl
        //ham truy vấn csdl
        public CategoriesViewComponent(DataContext context)
        {
            _dataContext = context;
        }
        // lấy dữ liệu
        public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Categories.ToListAsync());
    }
}
