using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class _1313123123oij1iopopaoskdopaskopda12312312 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Error",
                table: "AppStoredFiles");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "AppStoredFiles");

            migrationBuilder.DropColumn(
                name: "Synced",
                table: "AppFile");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "AppStoredFiles",
                type: "TEXT",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "AppStoredFiles",
                type: "TEXT",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Synced",
                table: "AppFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
