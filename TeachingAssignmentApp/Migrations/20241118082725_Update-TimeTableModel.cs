using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class UpdateTimeTableModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeTableModel_Classes_ClassId",
                table: "TimeTableModel");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeTableModel_TeachingAssignment_TeachingAssignmentId",
                table: "TimeTableModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeTableModel",
                table: "TimeTableModel");

            migrationBuilder.RenameTable(
                name: "TimeTableModel",
                newName: "TimeTableModels");

            migrationBuilder.RenameIndex(
                name: "IX_TimeTableModel_TeachingAssignmentId",
                table: "TimeTableModels",
                newName: "IX_TimeTableModels_TeachingAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeTableModel_ClassId",
                table: "TimeTableModels",
                newName: "IX_TimeTableModels_ClassId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeTableModels",
                table: "TimeTableModels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTableModels_Classes_ClassId",
                table: "TimeTableModels",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTableModels_TeachingAssignment_TeachingAssignmentId",
                table: "TimeTableModels",
                column: "TeachingAssignmentId",
                principalTable: "TeachingAssignment",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeTableModels_Classes_ClassId",
                table: "TimeTableModels");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeTableModels_TeachingAssignment_TeachingAssignmentId",
                table: "TimeTableModels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeTableModels",
                table: "TimeTableModels");

            migrationBuilder.RenameTable(
                name: "TimeTableModels",
                newName: "TimeTableModel");

            migrationBuilder.RenameIndex(
                name: "IX_TimeTableModels_TeachingAssignmentId",
                table: "TimeTableModel",
                newName: "IX_TimeTableModel_TeachingAssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeTableModels_ClassId",
                table: "TimeTableModel",
                newName: "IX_TimeTableModel_ClassId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeTableModel",
                table: "TimeTableModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTableModel_Classes_ClassId",
                table: "TimeTableModel",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTableModel_TeachingAssignment_TeachingAssignmentId",
                table: "TimeTableModel",
                column: "TeachingAssignmentId",
                principalTable: "TeachingAssignment",
                principalColumn: "Id");
        }
    }
}
