using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class TableProjectInput1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectAssignmentInput",
                table: "ProjectAssignmentInput");

            migrationBuilder.AlterColumn<string>(
                name: "TeacherCode",
                table: "ProjectAssignmentInput",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ProjectAssignmentInput",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectAssignmentInput",
                table: "ProjectAssignmentInput",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectAssignmentInput",
                table: "ProjectAssignmentInput");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProjectAssignmentInput");

            migrationBuilder.AlterColumn<string>(
                name: "TeacherCode",
                table: "ProjectAssignmentInput",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectAssignmentInput",
                table: "ProjectAssignmentInput",
                column: "TeacherCode");
        }
    }
}
