namespace GovernmentProcurement_ClassDB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modifyColumnLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Procurements", "TransmissionNumber", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Procurements", "TransmissionNumber", c => c.String(maxLength: 500));
        }
    }
}
