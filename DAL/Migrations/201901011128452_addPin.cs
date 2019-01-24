namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "MerchantPin", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "MerchantPin");
        }
    }
}
