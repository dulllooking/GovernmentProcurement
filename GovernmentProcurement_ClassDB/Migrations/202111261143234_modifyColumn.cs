namespace GovernmentProcurement_ClassDB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modifyColumn : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Procurements", "SubjectOfProcurement", c => c.String(maxLength: 500));
            AlterColumn("dbo.Procurements", "TransmissionNumber", c => c.String(maxLength: 500));
            DropColumn("dbo.Procurements", "JobNumber");
            DropColumn("dbo.Procurements", "IsHoliday");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Procurements", "IsHoliday", c => c.Boolean());
            AddColumn("dbo.Procurements", "JobNumber", c => c.String(maxLength: 50));
            AlterColumn("dbo.Procurements", "TransmissionNumber", c => c.Int());
            AlterColumn("dbo.Procurements", "SubjectOfProcurement", c => c.String(maxLength: 100));
        }
    }
}
