using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebSellingShoes.Repository.Validation;

namespace WebSellingShoes.Models
{
    public class SliderModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên slider")]
        public string Name { get; set; }
        public string Image { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string Description { get; set; }
        public int? Status { get; set; }
        [FileExtension]
        [NotMapped]
        public IFormFile? ImageUpload { get; set; }
    }
}
