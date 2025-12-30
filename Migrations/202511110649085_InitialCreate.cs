namespace Laptop88_3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CartItems",
                c => new
                    {
                        CartItemID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(),
                        ProductID = c.String(nullable: false, maxLength: 128),
                        Quantity = c.Int(nullable: false),
                        AddedAt = c.DateTime(nullable: false),
                        UserKey = c.String(),
                    })
                .PrimaryKey(t => t.CartItemID)
                .ForeignKey("dbo.Products", t => t.ProductID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.ProductID);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ProductID = c.String(nullable: false, maxLength: 128),
                        ProductName = c.String(),
                        ProductDescription = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Stock = c.Int(nullable: false),
                        color = c.String(),
                        CategoryID = c.Int(nullable: false),
                        ImageURL = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ProductID)
                .ForeignKey("dbo.Categories", t => t.CategoryID, cascadeDelete: true)
                .Index(t => t.CategoryID);
            
            CreateTable(
                "dbo.ProductGifts",
                c => new
                    {
                        ProductGiftID = c.Int(nullable: false, identity: true),
                        MainProductID = c.String(nullable: false, maxLength: 128),
                        GiftProductID = c.String(nullable: false, maxLength: 128),
                        PromotionID = c.Int(),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductGiftID)
                .ForeignKey("dbo.Products", t => t.GiftProductID)
                .ForeignKey("dbo.Products", t => t.MainProductID)
                .ForeignKey("dbo.Promotions", t => t.PromotionID)
                .Index(t => t.MainProductID)
                .Index(t => t.GiftProductID)
                .Index(t => t.PromotionID);
            
            CreateTable(
                "dbo.Promotions",
                c => new
                    {
                        PromotionID = c.Int(nullable: false, identity: true),
                        PromotionName = c.String(),
                        Description = c.String(),
                        DiscountPercent = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PricePromotion = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PromotionID);
            
            CreateTable(
                "dbo.ProductPromotions",
                c => new
                    {
                        ProductID = c.String(nullable: false, maxLength: 128),
                        PromotionID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ProductID, t.PromotionID })
                .ForeignKey("dbo.Products", t => t.ProductID, cascadeDelete: true)
                .ForeignKey("dbo.Promotions", t => t.PromotionID, cascadeDelete: true)
                .Index(t => t.ProductID)
                .Index(t => t.PromotionID);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.CategoryID);
            
            CreateTable(
                "dbo.LaptopSpecs",
                c => new
                    {
                        ProductID = c.String(nullable: false, maxLength: 128),
                        Brand = c.String(),
                        CPU = c.String(),
                        RAM = c.String(),
                        Storage = c.String(),
                        GraphicCard = c.String(),
                        Display = c.String(),
                        Series = c.String(),
                        OtherFeatures = c.String(),
                    })
                .PrimaryKey(t => t.ProductID)
                .ForeignKey("dbo.Products", t => t.ProductID)
                .Index(t => t.ProductID);
            
            CreateTable(
                "dbo.MouseSpecs",
                c => new
                    {
                        ProductID = c.String(nullable: false, maxLength: 128),
                        DPI = c.String(),
                        ConnectionType = c.String(),
                        Battery = c.String(),
                        Brand = c.String(),
                    })
                .PrimaryKey(t => t.ProductID)
                .ForeignKey("dbo.Products", t => t.ProductID)
                .Index(t => t.ProductID);
            
            CreateTable(
                "dbo.OrderDetails",
                c => new
                    {
                        OrderDetailID = c.Int(nullable: false, identity: true),
                        OrderID = c.Int(nullable: false),
                        ProductID = c.String(maxLength: 128),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.OrderDetailID)
                .ForeignKey("dbo.Orders", t => t.OrderID, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductID)
                .Index(t => t.OrderID)
                .Index(t => t.ProductID);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        OrderDate = c.DateTime(nullable: false),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(),
                        ShippingAddress = c.String(),
                        PhoneShipping = c.String(),
                    })
                .PrimaryKey(t => t.OrderID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentID = c.Int(nullable: false, identity: true),
                        OrderID = c.Int(nullable: false),
                        PaymentMethod = c.String(),
                        PaidAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaidAt = c.DateTime(nullable: false),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.PaymentID)
                .ForeignKey("dbo.Orders", t => t.OrderID, cascadeDelete: true)
                .Index(t => t.OrderID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        PasswordHash = c.String(),
                        Email = c.String(),
                        FullName = c.String(),
                        PhoneNumber = c.String(),
                        Address = c.String(),
                        Role = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UserID);
            
            CreateTable(
                "dbo.ShopProducts",
                c => new
                    {
                        ShopID = c.Int(nullable: false),
                        ProductID = c.String(nullable: false, maxLength: 128),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ShopID, t.ProductID })
                .ForeignKey("dbo.Products", t => t.ProductID, cascadeDelete: true)
                .ForeignKey("dbo.Shops", t => t.ShopID, cascadeDelete: true)
                .Index(t => t.ShopID)
                .Index(t => t.ProductID);
            
            CreateTable(
                "dbo.Shops",
                c => new
                    {
                        ShopID = c.Int(nullable: false, identity: true),
                        ShopName = c.String(),
                        Address = c.String(),
                    })
                .PrimaryKey(t => t.ShopID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ShopProducts", "ShopID", "dbo.Shops");
            DropForeignKey("dbo.ShopProducts", "ProductID", "dbo.Products");
            DropForeignKey("dbo.OrderDetails", "ProductID", "dbo.Products");
            DropForeignKey("dbo.Orders", "UserID", "dbo.Users");
            DropForeignKey("dbo.CartItems", "UserID", "dbo.Users");
            DropForeignKey("dbo.Payments", "OrderID", "dbo.Orders");
            DropForeignKey("dbo.OrderDetails", "OrderID", "dbo.Orders");
            DropForeignKey("dbo.MouseSpecs", "ProductID", "dbo.Products");
            DropForeignKey("dbo.LaptopSpecs", "ProductID", "dbo.Products");
            DropForeignKey("dbo.Products", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.CartItems", "ProductID", "dbo.Products");
            DropForeignKey("dbo.ProductGifts", "PromotionID", "dbo.Promotions");
            DropForeignKey("dbo.ProductPromotions", "PromotionID", "dbo.Promotions");
            DropForeignKey("dbo.ProductPromotions", "ProductID", "dbo.Products");
            DropForeignKey("dbo.ProductGifts", "MainProductID", "dbo.Products");
            DropForeignKey("dbo.ProductGifts", "GiftProductID", "dbo.Products");
            DropIndex("dbo.ShopProducts", new[] { "ProductID" });
            DropIndex("dbo.ShopProducts", new[] { "ShopID" });
            DropIndex("dbo.Payments", new[] { "OrderID" });
            DropIndex("dbo.Orders", new[] { "UserID" });
            DropIndex("dbo.OrderDetails", new[] { "ProductID" });
            DropIndex("dbo.OrderDetails", new[] { "OrderID" });
            DropIndex("dbo.MouseSpecs", new[] { "ProductID" });
            DropIndex("dbo.LaptopSpecs", new[] { "ProductID" });
            DropIndex("dbo.ProductPromotions", new[] { "PromotionID" });
            DropIndex("dbo.ProductPromotions", new[] { "ProductID" });
            DropIndex("dbo.ProductGifts", new[] { "PromotionID" });
            DropIndex("dbo.ProductGifts", new[] { "GiftProductID" });
            DropIndex("dbo.ProductGifts", new[] { "MainProductID" });
            DropIndex("dbo.Products", new[] { "CategoryID" });
            DropIndex("dbo.CartItems", new[] { "ProductID" });
            DropIndex("dbo.CartItems", new[] { "UserID" });
            DropTable("dbo.Shops");
            DropTable("dbo.ShopProducts");
            DropTable("dbo.Users");
            DropTable("dbo.Payments");
            DropTable("dbo.Orders");
            DropTable("dbo.OrderDetails");
            DropTable("dbo.MouseSpecs");
            DropTable("dbo.LaptopSpecs");
            DropTable("dbo.Categories");
            DropTable("dbo.ProductPromotions");
            DropTable("dbo.Promotions");
            DropTable("dbo.ProductGifts");
            DropTable("dbo.Products");
            DropTable("dbo.CartItems");
        }
    }
}
