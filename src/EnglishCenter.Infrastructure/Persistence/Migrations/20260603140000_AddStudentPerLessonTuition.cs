using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCenter.Infrastructure.Persistence.Migrations;

public partial class AddStudentPerLessonTuition : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "PerLessonTuition",
            table: "students",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PerLessonTuition",
            table: "Students");
    }
}
