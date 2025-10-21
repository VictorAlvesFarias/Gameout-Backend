using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class _130913123j12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppStoredFile_StoredFile_StoredFileId",
                table: "AppStoredFile");

            migrationBuilder.AlterColumn<int>(
                name: "StoredFileId",
                table: "AppStoredFile",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "AppStoredFile",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Processing",
                table: "AppStoredFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStoredFile_StoredFile_StoredFileId",
                table: "AppStoredFile",
                column: "StoredFileId",
                principalTable: "StoredFile",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppStoredFile_StoredFile_StoredFileId",
                table: "AppStoredFile");

            migrationBuilder.DropColumn(
                name: "Error",
                table: "AppStoredFile");

            migrationBuilder.DropColumn(
                name: "Processing",
                table: "AppStoredFile");

            migrationBuilder.AlterColumn<int>(
                name: "StoredFileId",
                table: "AppStoredFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStoredFile_StoredFile_StoredFileId",
                table: "AppStoredFile",
                column: "StoredFileId",
                principalTable: "StoredFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
