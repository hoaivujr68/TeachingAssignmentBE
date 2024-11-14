using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class TeachingAssignmentDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "TeacherCode",
                table: "Classes");

            migrationBuilder.CreateTable(
                name: "TeachingAssignment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingAssignment_Classes_Id",
                        column: x => x.Id,
                        principalTable: "Classes",
                        principalColumn: "Id");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeachingAssignment");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TeacherCode",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
