namespace Laptop88_3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOrderAddressStruct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "WardCode", c => c.String());
            AddColumn("dbo.Orders", "DistrictCode", c => c.String());
            AddColumn("dbo.Orders", "ProvinceCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "ProvinceCode");
            DropColumn("dbo.Orders", "DistrictCode");
            DropColumn("dbo.Orders", "WardCode");
        }
    }
}
