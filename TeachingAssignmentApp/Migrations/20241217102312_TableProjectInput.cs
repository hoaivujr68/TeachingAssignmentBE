using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingAssignmentApp.Migrations
{
    public partial class TableProjectInput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectAssignmentInput",
                columns: table => new
                {
                    TeacherCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Topic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesireAccept = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspiration1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspiration1Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspiration2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspiration2Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspiration3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aspiration3Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GdInstruct = table.Column<double>(type: "float", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAssignmentInput", x => x.TeacherCode);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectAssignmentInput");
        }
    }
}
