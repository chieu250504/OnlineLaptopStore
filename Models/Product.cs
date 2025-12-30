
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Laptop88_3.Models
{
    public class Product
    {
        [Key]
        [Required(ErrorMessage = "Vui lòng nhập mã sản phẩm.")]
        public string ProductID { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập giá.")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ.")]
        public int Stock { get; set; }
        public string color {  get; set; }
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryID { get; set; }
        public string ImageURL { get; set; }
        public DateTime CreatedAt { get; set; }
        // 🔹 Thêm thuộc tính kích thước và cân nặng
        [Range(0, double.MaxValue, ErrorMessage = "Cân nặng phải >= 0.")]
        public decimal Weight { get; set; }      // kg

        [Range(0, double.MaxValue, ErrorMessage = "Chiều dài phải >= 0.")]
        public decimal Length { get; set; }      // cm

        [Range(0, double.MaxValue, ErrorMessage = "Chiều rộng phải >= 0.")]
        public decimal Width { get; set; }       // cm

        [Range(0, double.MaxValue, ErrorMessage = "Chiều cao phải >= 0.")]
        public decimal Height { get; set; }      // cm

        public virtual LaptopSpecs LaptopSpecs { get; set; }
        public virtual MouseSpecs MouseSpecs { get; set; }

        // ✅ Quan hệ nhiều–nhiều với Promotion
        public virtual ICollection<ProductPromotion> ProductPromotions { get; set; }

        // ✅ Nhiều–nhiều với Shop (có ShopProduct)
        public virtual ICollection<ShopProduct> ShopProducts { get; set; }

        // ✅ Nhiều–nhiều với Orders qua OrderDetails
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        // ✅ Nhiều–nhiều với User Cart
        public virtual ICollection<CartItem> CartItems { get; set; }

        // ✅ 1 Product thuộc 1 Category
        public virtual Category Category { get; set; }
        // ✅ Danh sách quà mà sản phẩm này được tặng
        public virtual ICollection<ProductGift> ProductGifts { get; set; }

        // Các sản phẩm mà sản phẩm này là quà tặng cho chúng
        public virtual ICollection<ProductGift> AsGiftFor { get; set; }

    }
}
