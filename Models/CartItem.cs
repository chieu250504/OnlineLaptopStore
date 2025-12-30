using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Laptop88_3.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }

        // Cho phép null nếu người dùng chưa đăng nhập
        public int? UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [Required]
        public string ProductID { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Chỉ dùng khi UserID = null
        public string UserKey { get; set; }
    }
}
