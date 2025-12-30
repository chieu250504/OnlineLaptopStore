using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Laptop88_3.Models
{
    public class ProductPromotion
    {
        [Key, Column(Order = 0)]
        public string ProductID { get; set; }

        [Key, Column(Order = 1)]
        public int PromotionID { get; set; }

        // Navigation
        public virtual Product Product { get; set; }
        public virtual Promotion Promotion { get; set; }
    }
}