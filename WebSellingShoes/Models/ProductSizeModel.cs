using System.ComponentModel.DataAnnotations.Schema;

namespace WebSellingShoes.Models
{
    public class ProductSizeModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
