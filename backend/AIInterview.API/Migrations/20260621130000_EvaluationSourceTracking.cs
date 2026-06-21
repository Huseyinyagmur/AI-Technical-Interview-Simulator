using AIInterview.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.API.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260621130000_EvaluationSourceTracking")]
public partial class EvaluationSourceTracking : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Source", table: "AnswerEvaluations", type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "MissingEvaluation");
        migrationBuilder.AddColumn<string>(name: "ErrorMessage", table: "AnswerEvaluations", type: "nvarchar(max)", nullable: true);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Source", table: "AnswerEvaluations");
        migrationBuilder.DropColumn(name: "ErrorMessage", table: "AnswerEvaluations");
    }
}
