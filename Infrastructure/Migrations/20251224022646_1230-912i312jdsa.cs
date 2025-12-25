using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class _1230912i312jdsa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppFileLogs");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "ApplicationLogs",
                type: "INTEGER",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Action",
                table: "ApplicationLogs",
                type: "INTEGER",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "ApplicationLogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details",
                table: "ApplicationLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ApplicationLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "ApplicationLogs",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "AppFileLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ActionMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ActionType = table.Column<int>(type: "INTEGER", nullable: false),
                    AppFileId = table.Column<int>(type: "INTEGER", nullable: true),
                    AppStoredFileId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RecordName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    StoredFileId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFileLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppFileLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppFileLogs_UserId",
                table: "AppFileLogs",
                column: "UserId");
        }
    }
}
