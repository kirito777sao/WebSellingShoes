﻿namespace WebSellingShoes.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public int Status { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal ShippingCost { get; set; }
        public string CouponCode { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalPrice { get; set; } // Thêm trường TotalPrice
    }
}
