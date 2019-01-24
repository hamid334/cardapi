namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class merchantn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Boxes", "MerchantId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Boxes", "MerchantId");
        }
    }
}
