using AIInterview.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.API.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260621150000_InterviewTracks")]
public partial class InterviewTracks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Track", table: "InterviewSessions", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "General");
        migrationBuilder.AddColumn<string>(name: "Domain", table: "InterviewQuestions", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "General");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Track", table: "InterviewSessions");
        migrationBuilder.DropColumn(name: "Domain", table: "InterviewQuestions");
    }
}
