using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laptop88_3.Models
{
    public class Promotion
    {
        [Key]
        public int PromotionID { get; set; }

        public string PromotionName { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal PricePromotion {  get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int DisplayType { get; set; } = 1;

        // Navigation: many-to-many
        public virtual ICollection<ProductPromotion> ProductPromotions { get; set; }
    }
}