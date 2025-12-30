namespace Laptop88_3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeOrderAddressToName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "WardName", c => c.String());
            AddColumn("dbo.Orders", "DistrictName", c => c.String());
            AddColumn("dbo.Orders", "ProvinceName", c => c.String());
            DropColumn("dbo.Orders", "WardCode");
            DropColumn("dbo.Orders", "DistrictCode");
            DropColumn("dbo.Orders", "ProvinceCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "ProvinceCode", c => c.String());
            AddColumn("dbo.Orders", "DistrictCode", c => c.String());
            AddColumn("dbo.Orders", "WardCode", c => c.String());
            DropColumn("dbo.Orders", "ProvinceName");
            DropColumn("dbo.Orders", "DistrictName");
            DropColumn("dbo.Orders", "WardName");
        }
    }
}
