namespace GovernmentProcurement_ClassDB.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBinit : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Procurements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProcuringEntity = c.String(maxLength: 50),
                        JobNumber = c.String(maxLength: 50),
                        SubjectOfProcurement = c.String(maxLength: 100),
                        TransmissionNumber = c.Int(),
                        TypeOfTendering = c.String(maxLength: 50),
                        TypeOfProcurement = c.String(maxLength: 50),
                        DateOfPublication = c.DateTime(),
                        DateLimitOfTenders = c.DateTime(),
                        IsHoliday = c.Boolean(),
                        Budget = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Procurements");
        }
    }
}
