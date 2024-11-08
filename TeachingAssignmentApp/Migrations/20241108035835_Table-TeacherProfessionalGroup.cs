using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class TableTeacherProfessionalGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_ProfessionalGroup_TeacherId",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfessionalGroup_Teacher_TeacherId",
                table: "ProfessionalGroup");

            migrationBuilder.DropIndex(
                name: "IX_ProfessionalGroup_TeacherId",
                table: "ProfessionalGroup");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "ProfessionalGroup");

            migrationBuilder.CreateTable(
                name: "TeacherProfessionalGroup",
                columns: table => new
                {
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfessionalGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherProfessionalGroup", x => new { x.TeacherId, x.ProfessionalGroupId });
                    table.ForeignKey(
                        name: "FK_TeacherProfessionalGroup_ProfessionalGroup_ProfessionalGroupId",
                        column: x => x.ProfessionalGroupId,
                        principalTable: "ProfessionalGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherProfessionalGroup_Teacher_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teacher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Course_ProfessionalGroupId",
                table: "Course",
                column: "ProfessionalGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherProfessionalGroup_ProfessionalGroupId",
                table: "TeacherProfessionalGroup",
                column: "ProfessionalGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_ProfessionalGroup_ProfessionalGroupId",
                table: "Course",
                column: "ProfessionalGroupId",
                principalTable: "ProfessionalGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_ProfessionalGroup_ProfessionalGroupId",
                table: "Course");

            migrationBuilder.DropTable(
                name: "TeacherProfessionalGroup");

            migrationBuilder.DropIndex(
                name: "IX_Course_ProfessionalGroupId",
                table: "Course");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "ProfessionalGroup",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalGroup_TeacherId",
                table: "ProfessionalGroup",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_ProfessionalGroup_TeacherId",
                table: "Course",
                column: "TeacherId",
                principalTable: "ProfessionalGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessionalGroup_Teacher_TeacherId",
                table: "ProfessionalGroup",
                column: "TeacherId",
                principalTable: "Teacher",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
