using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class _1k12po3k12po3k12asdasddas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppStoredFile_AppFile_AppFileId",
                table: "AppStoredFile");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStoredFile_AspNetUsers_UserId",
                table: "AppStoredFile");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStoredFile_StoredFile_StoredFileId",
                table: "AppStoredFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppStoredFile",
                table: "AppStoredFile");

            migrationBuilder.RenameTable(
                name: "AppStoredFile",
                newName: "AppStoredFiles");

            migrationBuilder.RenameIndex(
                name: "IX_AppStoredFile_UserId",
                table: "AppStoredFiles",
                newName: "IX_AppStoredFiles_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AppStoredFile_StoredFileId",
                table: "AppStoredFiles",
                newName: "IX_AppStoredFiles_StoredFileId");

            migrationBuilder.RenameIndex(
                name: "IX_AppStoredFile_AppFileId",
                table: "AppStoredFiles",
                newName: "IX_AppStoredFiles_AppFileId");

            migrationBuilder.AlterColumn<string>(
                name: "Error",
                table: "AppStoredFiles",
                type: "TEXT",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppStoredFiles",
                table: "AppStoredFiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppStoredFiles_AppFile_AppFileId",
                table: "AppStoredFiles",
                column: "AppFileId",
                principalTable: "AppFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStoredFiles_AspNetUsers_UserId",
                table: "AppStoredFiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStoredFiles_StoredFile_StoredFileId",
                table: "AppStoredFiles",
                column: "StoredFileId",
                principalTable: "StoredFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppStoredFiles_AppFile_AppFileId",
                table: "AppStoredFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStoredFiles_AspNetUsers_UserId",
                table: "AppStoredFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStoredFiles_StoredFile_StoredFileId",
                table: "AppStoredFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppStoredFiles",
                table: "AppStoredFiles");

            migrationBuilder.RenameTable(
                name: "AppStoredFiles",
                newName: "AppStoredFile");

            migrationBuilder.RenameIndex(
                name: "IX_AppStoredFiles_UserId",
                table: "AppStoredFile",
                newName: "IX_AppStoredFile_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AppStoredFiles_StoredFileId",
                table: "AppStoredFile",
                newName: "IX_AppStoredFile_StoredFileId");

            migrationBuilder.RenameIndex(
                name: "IX_AppStoredFiles_AppFileId",
                table: "AppStoredFile",
                newName: "IX_AppStoredFile_AppFileId");

            migrationBuilder.AlterColumn<string>(
                name: "Error",
                table: "AppStoredFile",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppStoredFile",
                table: "AppStoredFile",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppStoredFile_AppFile_AppFileId",
                table: "AppStoredFile",
                column: "AppFileId",
                principalTable: "AppFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStoredFile_AspNetUsers_UserId",
                table: "AppStoredFile",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStoredFile_StoredFile_StoredFileId",
                table: "AppStoredFile",
                column: "StoredFileId",
                principalTable: "StoredFile",
                principalColumn: "Id");
        }
    }
}
