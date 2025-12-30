namespace Laptop88_3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDisplayTypeToPromotion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Promotions", "DisplayType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Promotions", "DisplayType");
        }
    }
}
