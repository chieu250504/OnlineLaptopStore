using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Laptop88_3.Models
{
    public class MouseSpecs
    {
        [Key, ForeignKey("Product")]
        public string ProductID { get; set; }

        public string DPI { get; set; }
        public string ConnectionType { get; set; }
        public string Battery { get; set; }
        public string Brand { get; set; }
        public virtual Product Product { get; set; }
    }
}