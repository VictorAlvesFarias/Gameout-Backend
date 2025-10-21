using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class _1paoskdopask312312 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Erro",
                table: "AppStoredFiles");

            migrationBuilder.RenameColumn(
                name: "Mensagem",
                table: "AppStoredFiles",
                newName: "Message");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Message",
                table: "AppStoredFiles",
                newName: "Mensagem");

            migrationBuilder.AddColumn<bool>(
                name: "Erro",
                table: "AppStoredFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
