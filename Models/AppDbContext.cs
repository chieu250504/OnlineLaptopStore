using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Laptop88_3.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("Laptop88_3ConnectoString") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<LaptopSpecs> LaptopSpecs { get; set; }
        public DbSet<MouseSpecs> MouseSpecs { get; set; }
        public DbSet<ProductPromotion> ProductPromotions { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<ShopProduct> ShopProducts { get; set; }
        public DbSet<ProductGift> ProductGifts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Cấu hình khóa chính cho ProductPromotion
            modelBuilder.Entity<ProductPromotion>()
                .HasKey(pp => new { pp.ProductID, pp.PromotionID });

            // ✅ Cấu hình quan hệ Product - ProductGift (MainProduct)
            modelBuilder.Entity<ProductGift>()
                .HasRequired(pg => pg.MainProduct)
                .WithMany(p => p.ProductGifts)
                .HasForeignKey(pg => pg.MainProductID)
                .WillCascadeOnDelete(false);

            // ✅ Cấu hình quan hệ Product - ProductGift (GiftProduct)
            modelBuilder.Entity<ProductGift>()
                .HasRequired(pg => pg.GiftProduct)
                .WithMany(p => p.AsGiftFor)
                .HasForeignKey(pg => pg.GiftProductID)
                .WillCascadeOnDelete(false);

            // ✅ Cấu hình quan hệ Promotion - ProductGift (1-n)
            modelBuilder.Entity<ProductGift>()
                .HasOptional(pg => pg.Promotion)
                .WithMany()
                .HasForeignKey(pg => pg.PromotionID)
                .WillCascadeOnDelete(false);
        }
    }
}
