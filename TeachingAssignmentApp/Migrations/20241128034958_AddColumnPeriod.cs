using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class AddColumnPeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PeriodJson",
                table: "TimeTableModels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeriodJson",
                table: "TimeTableModels");
        }
    }
}
