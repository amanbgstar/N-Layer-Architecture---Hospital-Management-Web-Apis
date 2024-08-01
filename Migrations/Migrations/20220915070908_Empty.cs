using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagement.Domain.Migrations
{
    public partial class Empty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE sp_GetPatientLabTestList  
                        @pageNo INT, @pageCount INT, @total INT OUTPUT  
                        AS  
                        BEGIN 
	                        DECLARE @offset INT
   
                        IF(@pageNo=0)
                              BEGIN
                                SET @offset = @pageNo
                               END
                            ELSE 
                              BEGIN
                                SET @offset = @pageNo*@pageCount
                              END
 
                        SET NOCOUNT ON;  
                        SELECT * FROM Labtests ORDER BY TestId ASC OFFSET 
                        @offset ROWS FETCH NEXT @pageCount ROWS ONLY
                        SELECT @total = COUNT(TestId) FROM Labtests   
                        RETURN  
                        END";

            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
