using System;
using AIInterview.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIInterview.API.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260621120000_PortfolioEnhancements")]
public partial class PortfolioEnhancements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(name: "CompletedAtUtc", table: "InterviewSessions", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<string>(name: "Concept", table: "InterviewQuestions", type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "General");
        migrationBuilder.AddColumn<string>(name: "Difficulty", table: "InterviewQuestions", type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Junior");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "CompletedAtUtc", table: "InterviewSessions");
        migrationBuilder.DropColumn(name: "Concept", table: "InterviewQuestions");
        migrationBuilder.DropColumn(name: "Difficulty", table: "InterviewQuestions");
    }
}
