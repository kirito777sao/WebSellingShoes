using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebSellingShoes.Repository.Validation;

namespace WebSellingShoes.Models
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MinLength(4, ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string Name { get; set; }
        [Required, MinLength(4, ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        public string Description { get; set; }
        public string Slug { get; set; }
        public string Image { get; set; }        
        [Range(1000, 10000000000, ErrorMessage = "Giá sản phẩm từ 1000 đến 1000000000")]
        [Column(TypeName = "decimal(18, 2)")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Giá sản phẩm phải là số")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập giá nhập vào của sản phẩm")]
        public decimal CapitalPrice { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn 1 thương hiệu")]
        public int BrandId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn 1 danh mục")]
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public CategoryModel Category { get; set; }
        public BrandModel Brand { get; set; }
        public List<RatingModel> Ratings { get; set; }        
        [FileExtension]
        [NotMapped]
        public IFormFile? ImageUpload { get; set; }
        public List<ProductSizeModel> ProductSizes { get; set; }
        public List<ProductImageModel> ProductImages { get; set; } = new List<ProductImageModel>();
    }
}
