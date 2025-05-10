using Microsoft.AspNetCore.Identity;

namespace WebSellingShoes.Models
{
    public class AppUserModel : IdentityUser
    {
        public string RoleId { get; set; }
        public string Token { get; set; }
    }
}
