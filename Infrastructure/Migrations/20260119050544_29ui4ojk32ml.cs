using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class _29ui4ojk32ml : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AppStoredFiles");

            migrationBuilder.DropColumn(
                name: "StatusDetails",
                table: "AppStoredFiles");

            migrationBuilder.DropColumn(
                name: "StatusMessage",
                table: "AppStoredFiles");

            migrationBuilder.DropColumn(
                name: "Versioned",
                table: "AppStoredFiles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AppStoredFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatusDetails",
                table: "AppStoredFiles",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusMessage",
                table: "AppStoredFiles",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Versioned",
                table: "AppStoredFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
