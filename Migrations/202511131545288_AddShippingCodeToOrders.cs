namespace Laptop88_3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShippingCodeToOrders : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "ShippingCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "ShippingCode");
        }
    }
}
