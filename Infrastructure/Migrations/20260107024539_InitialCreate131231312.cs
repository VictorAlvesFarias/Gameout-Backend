using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class InitialCreate131231312 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StoredFile",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StoredFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AppStoredFiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppStoredFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AppFile",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StoredFile");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StoredFile");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AppStoredFiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppStoredFiles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AppFile");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppFile");
        }
    }
}
