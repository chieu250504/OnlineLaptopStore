using System;
using System.Collections.Generic;
using System.Data.Entity;
using Laptop88_3.Models;
using Laptop88_3.Services;

namespace Laptop88_3.Data
{
    public class DbInitializer : DropCreateDatabaseIfModelChanges<AppDbContext>
    {
        protected override void Seed(AppDbContext context)
        {
            
            var users = new List<User>
            {
                new User { UserID = 1, Username = "admin", PasswordHash = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", Email="admin@laptop88.vn", FullName="Quản trị viên", PhoneNumber="0900000000", Address="Hà Nội", Role="Admin", CreatedAt=DateTime.Now },
                new User { UserID = 2, Username = "staff", PasswordHash = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", Email="staff@laptop88.vn", FullName="Nhân Viên Bán Hàng", PhoneNumber="0911111111", Address="Hà Nội", Role="Staff", CreatedAt=DateTime.Now }
            };
            users.ForEach(u => context.Users.Add(u));
            context.SaveChanges();

            
            var categories = new List<Category>
            {
                new Category { CategoryID = 1, Name="Gaming"},
                new Category { CategoryID = 2, Name="Student"},
                new Category { CategoryID = 3, Name="High"},
                new Category { CategoryID = 4, Name="Mouse"}
            };
            categories.ForEach(c => context.Categories.Add(c));
            context.SaveChanges();

            
            var products = new List<Product>
            {
                new Product { ProductID="ACAS01", ProductName="Acer Aspire 5 A515-58M", ProductDescription="CPU i5-12450H, RAM 8GB", Price=14900000, Stock=10, CategoryID=1, CreatedAt=DateTime.Now,ImageURL="pr5.jpg" },
                new Product { ProductID="ACAS02", ProductName="Acer Aspire 5 A515-58M", ProductDescription="CPU i5-13420H, RAM 16GB", Price=15900000, Stock=10, CategoryID=1, CreatedAt=DateTime.Now, ImageURL="pr6.jpg" },
                new Product { ProductID="MOU001", ProductName="Logitech Mouse", ProductDescription="Wireless Mouse", Price=250000, Stock=50, CategoryID=4, CreatedAt=DateTime.Now, ImageURL="mouse1.jpg" }
            };
            products.ForEach(p => context.Products.Add(p));
            context.SaveChanges();

            
            var laptopSpecs = new List<LaptopSpecs>
            {
                new LaptopSpecs { ProductID="ACAS01", Brand="Acer", CPU="i5-12450H", RAM="8GB", Storage="512GB SSD", GraphicCard="Intel UHD", Display="15.6 inch FHD" },
                new LaptopSpecs { ProductID="ACAS02", Brand="Acer", CPU="i5-13420H", RAM="16GB", Storage="512GB SSD", GraphicCard="Intel UHD", Display="15.6 inch FHD" }
            };
            laptopSpecs.ForEach(l => context.LaptopSpecs.Add(l));
            context.SaveChanges();

            
            var mouseSpecs = new List<MouseSpecs>
            {
                new MouseSpecs { ProductID="MOU001", Brand="Logitech", DPI="1000", ConnectionType="Wireless", Battery="AA Battery" }
            };
            mouseSpecs.ForEach(m => context.MouseSpecs.Add(m));
            context.SaveChanges();

            
            var promotions = new List<Promotion>
            {
                new Promotion { PromotionID=1, PromotionName="Khuyến mãi Giáng Sinh", Description="Giảm giá và tặng quà", DiscountPercent=10, PricePromotion=0, StartDate=DateTime.Now, EndDate=DateTime.Now.AddMonths(1), IsActive=true, DisplayType=1 }
            };
            promotions.ForEach(pr => context.Promotions.Add(pr));
            context.SaveChanges();

            
            var productPromotions = new List<ProductPromotion>
            {
                new ProductPromotion { ProductID="ACAS01", PromotionID=1 },
                new ProductPromotion { ProductID="ACAS02", PromotionID=1 }
            };
            productPromotions.ForEach(pp => context.ProductPromotions.Add(pp));
            context.SaveChanges();

            
            var cartItems = new List<CartItem>
            {
                new CartItem { UserID=1, ProductID="ACAS01", Quantity=1, AddedAt=DateTime.Now },
                new CartItem { UserID=2, ProductID="MOU001", Quantity=2, AddedAt=DateTime.Now }
            };
            cartItems.ForEach(ci => context.CartItems.Add(ci));
            context.SaveChanges();
        }
    }
}
