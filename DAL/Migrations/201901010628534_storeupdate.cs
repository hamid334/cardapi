namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class storeupdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stores", "MerchantEmail", c => c.String());
            AddColumn("dbo.Stores", "MerchantPhone", c => c.String());
            AddColumn("dbo.Stores", "MerchantPin", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stores", "MerchantPin");
            DropColumn("dbo.Stores", "MerchantPhone");
            DropColumn("dbo.Stores", "MerchantEmail");
        }
    }
}
