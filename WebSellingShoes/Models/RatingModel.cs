using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSellingShoes.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên người đánh giá")]
        public string UserName { get; set; }
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }        
        [Range(1, 5, ErrorMessage = "Số sao đánh giá từ 1 đến 5")]        
        public int Star { get; set; }        
        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
