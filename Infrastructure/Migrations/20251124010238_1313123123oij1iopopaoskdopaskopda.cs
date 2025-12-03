using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class _1313123123oij1iopopaoskdopaskopda : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Processing",
                table: "AppStoredFiles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Processing",
                table: "AppStoredFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
