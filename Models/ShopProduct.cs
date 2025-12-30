using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laptop88_3.Models
{
    public class ShopProduct
    {
        [Key, Column(Order = 0)]
        public int ShopID { get; set; }

        [Key, Column(Order = 1)]
        public string ProductID { get; set; }

        public int Quantity { get; set; }

        // Navigation
        public virtual Shop Shop { get; set; }
        public virtual Product Product { get; set; }
    }
}