using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class TeachingAssignmentDBAdd1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeachingAssignment_Classes_Id",
                table: "TeachingAssignment");

            migrationBuilder.AddColumn<Guid>(
                name: "TeachingAssignmentId",
                table: "TimeTableModel",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "TeachingAssignment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CourseName",
                table: "TeachingAssignment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "GdTeaching",
                table: "TeachingAssignment",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "TeachingAssignment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxEnrol",
                table: "TeachingAssignment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TeachingAssignment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeTable",
                table: "TeachingAssignment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "TeachingAssignment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTableModel_TeachingAssignmentId",
                table: "TimeTableModel",
                column: "TeachingAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTableModel_TeachingAssignment_TeachingAssignmentId",
                table: "TimeTableModel",
                column: "TeachingAssignmentId",
                principalTable: "TeachingAssignment",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeTableModel_TeachingAssignment_TeachingAssignmentId",
                table: "TimeTableModel");

            migrationBuilder.DropIndex(
                name: "IX_TimeTableModel_TeachingAssignmentId",
                table: "TimeTableModel");

            migrationBuilder.DropColumn(
                name: "TeachingAssignmentId",
                table: "TimeTableModel");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "TeachingAssignment");

            migrationBuilder.DropColumn(
                name: "CourseName",
                table: "TeachingAssignment");

            migrationBuilder.DropColumn(
                name: "GdTeaching",
                table: "TeachingAssignment");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "TeachingAssignment");

            migrationBuilder.DropColumn(
                name: "MaxEnrol",
                table: "TeachingAssignment");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TeachingAssignment");

            migrationBuilder.DropColumn(
                name: "TimeTable",
                table: "TeachingAssignment");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "TeachingAssignment");

            migrationBuilder.AddForeignKey(
                name: "FK_TeachingAssignment_Classes_Id",
                table: "TeachingAssignment",
                column: "Id",
                principalTable: "Classes",
                principalColumn: "Id");
        }
    }
}
