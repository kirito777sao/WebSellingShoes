using System.ComponentModel.DataAnnotations;

namespace WebSellingShoes.Models.ViewModels
{
    public class LoginViewModel
    {
        //tại sao không sử dụng UserModel ở đây?
        //vì UserModel chỉ là model dữ liệu, không chứa các thuộc tính như ReturnUrl
        // tại sao lại cần ReturnUrl?
        // vì khi người dùng truy cập vào trang admin mà chưa đăng nhập, hệ thống sẽ chuyển hướng người dùng đến trang login,
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập UserName")]
        public string UserName { get; set; }
        [DataType(DataType.Password), Required(ErrorMessage = "Vui lòng nhập Password")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
