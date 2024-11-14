using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class AddColGdTeacher : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "GdTeaching",
                table: "Classes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GdTeaching",
                table: "Classes");
        }
    }
}
