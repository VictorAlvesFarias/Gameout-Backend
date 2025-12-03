using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class _12313213123opk12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AppFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatusDetails",
                table: "AppFile",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusMessage",
                table: "AppFile",
                type: "TEXT",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "Status",
                table: "AppFile");

            migrationBuilder.DropColumn(
                name: "StatusDetails",
                table: "AppFile");

            migrationBuilder.DropColumn(
                name: "StatusMessage",
                table: "AppFile");
        }
    }
}
