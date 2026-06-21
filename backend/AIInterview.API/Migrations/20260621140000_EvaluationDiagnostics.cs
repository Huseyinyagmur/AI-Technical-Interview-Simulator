using AIInterview.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.API.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260621140000_EvaluationDiagnostics")]
public partial class EvaluationDiagnostics : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<string>(name: "RawGeminiResponse", table: "AnswerEvaluations", type: "nvarchar(max)", nullable: true);
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "RawGeminiResponse", table: "AnswerEvaluations");
}
