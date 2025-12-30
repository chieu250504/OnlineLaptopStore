using Laptop88_3.Models;
using System;

public class ProductGift
{
    public int ProductGiftID { get; set; }

    // Khóa ngoại trỏ đến sản phẩm chính
    public string MainProductID { get; set; }
    public virtual Product MainProduct { get; set; }

    // Khóa ngoại trỏ đến sản phẩm quà tặng
    public string GiftProductID { get; set; }
    public virtual Product GiftProduct { get; set; }

    // Khóa ngoại trỏ đến khuyến mãi
    public int? PromotionID { get; set; }
    public virtual Promotion Promotion { get; set; }

    public int Quantity { get; set; }



}
