using System.ComponentModel.DataAnnotations;

namespace WebSellingShoes.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu không được để trống mã caupon")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Yêu cầu không được để trống mô tả mã caupon")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Yêu cầu không được để trống giá trị mã caupon")]
        [Range(0, 100, ErrorMessage = "Giá trị mã caupon phải từ 0 đến 100")]
        public int Percent { get; set; }        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set;}
        [Required(ErrorMessage = "Yêu cầu không được để trống số lượng caupon")]
        public int Quantity { get; set; }
        public int Status { get; set; }
    }
}
