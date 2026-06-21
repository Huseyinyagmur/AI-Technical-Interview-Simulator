using AIInterview.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AIInterview.API.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20260621160000_AddUserAuthentication")]
public partial class AddUserAuthentication : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "Users", columns: table => new { Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false), Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false), PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false), CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false) }, constraints: table => table.PrimaryKey("PK_Users", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);
        migrationBuilder.AddColumn<Guid>(name: "UserId", table: "InterviewSessions", type: "uniqueidentifier", nullable: true);
        migrationBuilder.CreateIndex(name: "IX_InterviewSessions_UserId", table: "InterviewSessions", column: "UserId");
        migrationBuilder.AddForeignKey(name: "FK_InterviewSessions_Users_UserId", table: "InterviewSessions", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
    }
    protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropForeignKey("FK_InterviewSessions_Users_UserId", "InterviewSessions"); migrationBuilder.DropIndex("IX_InterviewSessions_UserId", "InterviewSessions"); migrationBuilder.DropColumn("UserId", "InterviewSessions"); migrationBuilder.DropTable("Users"); }
}
