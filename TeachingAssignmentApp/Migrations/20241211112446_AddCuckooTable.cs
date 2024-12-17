using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class AddCuckooTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CuckooTeachingAssignmentId",
                table: "TimeTableModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CuckooProjectAssignment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeacherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Topic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesireAccept = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspiration1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspiration2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspiration3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GdInstruct = table.Column<double>(type: "float", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuckooProjectAssignment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuckooTeachingAssignment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxEnrol = table.Column<int>(type: "int", nullable: true),
                    TimeTable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GdTeaching = table.Column<double>(type: "float", nullable: true),
                    TeacherCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeachingName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuckooTeachingAssignment", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTableModels_CuckooTeachingAssignmentId",
                table: "TimeTableModels",
                column: "CuckooTeachingAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTableModels_CuckooTeachingAssignment_CuckooTeachingAssignmentId",
                table: "TimeTableModels",
                column: "CuckooTeachingAssignmentId",
                principalTable: "CuckooTeachingAssignment",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeTableModels_CuckooTeachingAssignment_CuckooTeachingAssignmentId",
                table: "TimeTableModels");

            migrationBuilder.DropTable(
                name: "CuckooProjectAssignment");

            migrationBuilder.DropTable(
                name: "CuckooTeachingAssignment");

            migrationBuilder.DropIndex(
                name: "IX_TimeTableModels_CuckooTeachingAssignmentId",
                table: "TimeTableModels");

            migrationBuilder.DropColumn(
                name: "CuckooTeachingAssignmentId",
                table: "TimeTableModels");
        }
    }
}
