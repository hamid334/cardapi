namespace DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class merchant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "CategoryId", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "Country", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.Users", "Country");
            DropColumn("dbo.Users", "CategoryId");
        }
    }
}
