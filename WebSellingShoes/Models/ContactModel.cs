using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebSellingShoes.Repository.Validation;

namespace WebSellingShoes.Models
{
    public class ContactModel
    {
        [Key]
        [Required(ErrorMessage = "Yêu cầu nhập tiêu đề liên hệ")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập địa chỉ email liên hệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại liên hệ")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập nội dung liên hệ")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập địa chỉ liên hệ")]
        public string Map { get; set; }

        public string LogoImg { get; set; }
        [FileExtension]
        [NotMapped]
        public IFormFile? ImageUpload { get; set; }
    }
}
