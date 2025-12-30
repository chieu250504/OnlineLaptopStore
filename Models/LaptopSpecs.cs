using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Laptop88_3.Models
{
    
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class LaptopSpecs
    {
        [Key]
        [ForeignKey("Product")]
        public string ProductID { get; set; }

        public string Brand { get; set; }
        public string CPU { get; set; }
        public string RAM { get; set; }
        public string Storage { get; set; }
        public string GraphicCard { get; set; }
        public string Display { get; set; }
        public string Series { get; set; }
        public string OtherFeatures { get; set; }

        public virtual Product Product { get; set; }
    }

}