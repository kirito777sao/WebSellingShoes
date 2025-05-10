using System.ComponentModel.DataAnnotations.Schema;

namespace WebSellingShoes.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
