using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebSellingShoes.Models
{
    public class ProductImageModel
    {
        [Key]
        public int Id { get; set; }
        public string ImageName { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
