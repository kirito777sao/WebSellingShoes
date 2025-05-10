using System.ComponentModel.DataAnnotations;

namespace WebSellingShoes.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public int Id { get; set; }
        public ProductModel ProductDetails { get; set; }
        public List<ProductSizeModel> ProductSizes { get; set; }
        public int Quantity { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên người đánh giá")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }       
        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        public string Comment { get; set; }
        public List<RatingModel> Ratings { get; set; }
        public int Star { get; set; } // Thêm thuộc tính Star
    }
}
