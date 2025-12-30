namespace Laptop88_3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeOrderAddressToCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "WardCode", c => c.String());
            AddColumn("dbo.Orders", "DistrictCode", c => c.String());
            AddColumn("dbo.Orders", "ProvinceCode", c => c.String());
            DropColumn("dbo.Orders", "WardName");
            DropColumn("dbo.Orders", "DistrictName");
            DropColumn("dbo.Orders", "ProvinceName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "ProvinceName", c => c.String());
            AddColumn("dbo.Orders", "DistrictName", c => c.String());
            AddColumn("dbo.Orders", "WardName", c => c.String());
            DropColumn("dbo.Orders", "ProvinceCode");
            DropColumn("dbo.Orders", "DistrictCode");
            DropColumn("dbo.Orders", "WardCode");
        }
    }
}
