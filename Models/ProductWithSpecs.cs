using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laptop88_3.Models
{
    public class ProductWithSpecs
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public string ImageURL { get; set; }

        // Specs
        public string Brand { get; set; }
        public string CPU { get; set; }
        public string RAM { get; set; }
        public string GraphicCard { get; set; }
        public string Display { get; set; }
        public string Storage { get; set; }
        //mouse

        public string DPI { get; set; }
        public string ConnectionType { get; set; }
        public string Battery { get; set; }
        //promotion
        public decimal? DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal? PromoProgressPercent { get; set; }
        public List<ProductWithSpecs> ProductGifts { get; set; }

        public List<ProductWithSpecs> RelatedProducts { get; set; }
        public DateTime? PromoEndDate { get; set; }
        public int? DisplayType { get; set; }
        public int? PromotionID { get; set; }
        public string PromotionName { get; set; }
    }
}