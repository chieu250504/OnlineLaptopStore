
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laptop88_3.Models
{
    public class Shop
    {
        [Key]
        public int ShopID { get; set; }

        public string ShopName { get; set; }
        public string Address { get; set; }

        // Navigation property
        public virtual ICollection<ShopProduct> ShopProducts { get; set; }
    }
}