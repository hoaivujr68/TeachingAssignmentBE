using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class AddColumnName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProjectAssigment",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProjectAssigment");
        }
    }
}
