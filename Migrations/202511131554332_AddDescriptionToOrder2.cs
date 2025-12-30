namespace Laptop88_3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDescriptionToOrder2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "DescriptionUser", c => c.String());
            AddColumn("dbo.Orders", "DescriptionAdmin", c => c.String());
           
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "Description", c => c.String());
            DropColumn("dbo.Orders", "DescriptionAdmin");
            DropColumn("dbo.Orders", "DescriptionUser");
        }
    }
}
