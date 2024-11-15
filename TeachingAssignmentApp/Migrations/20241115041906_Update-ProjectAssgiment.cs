using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class UpdateProjectAssgiment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ProjectAssigment",
                newName: "Topic");

            migrationBuilder.RenameColumn(
                name: "StudenId",
                table: "ProjectAssigment",
                newName: "TeacherName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ProjectAssigment",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "CourseName",
                table: "ProjectAssigment",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "ProjectAssigment",
                newName: "DesireAccept");

            migrationBuilder.AlterColumn<string>(
                name: "TeacherCode",
                table: "ProjectAssigment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Aspiration1",
                table: "ProjectAssigment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Aspiration2",
                table: "ProjectAssigment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Aspiration3",
                table: "ProjectAssigment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "ProjectAssigment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "ProjectAssigment",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aspiration1",
                table: "ProjectAssigment");

            migrationBuilder.DropColumn(
                name: "Aspiration2",
                table: "ProjectAssigment");

            migrationBuilder.DropColumn(
                name: "Aspiration3",
                table: "ProjectAssigment");

            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "ProjectAssigment");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "ProjectAssigment");

            migrationBuilder.RenameColumn(
                name: "Topic",
                table: "ProjectAssigment",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "TeacherName",
                table: "ProjectAssigment",
                newName: "StudenId");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "ProjectAssigment",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ProjectAssigment",
                newName: "CourseName");

            migrationBuilder.RenameColumn(
                name: "DesireAccept",
                table: "ProjectAssigment",
                newName: "Code");

            migrationBuilder.AlterColumn<string>(
                name: "TeacherCode",
                table: "ProjectAssigment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
