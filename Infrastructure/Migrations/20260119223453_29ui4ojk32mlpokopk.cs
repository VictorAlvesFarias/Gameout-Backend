using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class _29ui4ojk32mlpokopk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StoredFile");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppStoredFiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppFile");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "StoredFile",
                newName: "DaleteDate");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "AppStoredFiles",
                newName: "DaleteDate");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "AppFile",
                newName: "DaleteDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "DaleteDate",
                table: "UserApiKeys",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DaleteDate",
                table: "Traces",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "StoredFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<DateTime>(
                name: "DaleteDate",
                table: "ContextTraces",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DaleteDate",
                table: "ApplicationLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "AppFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaleteDate",
                table: "UserApiKeys");

            migrationBuilder.DropColumn(
                name: "DaleteDate",
                table: "Traces");

            migrationBuilder.DropColumn(
                name: "DaleteDate",
                table: "ContextTraces");

            migrationBuilder.DropColumn(
                name: "DaleteDate",
                table: "ApplicationLogs");

            migrationBuilder.RenameColumn(
                name: "DaleteDate",
                table: "StoredFile",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DaleteDate",
                table: "AppStoredFiles",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DaleteDate",
                table: "AppFile",
                newName: "DeletedAt");

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "StoredFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StoredFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppStoredFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Deleted",
                table: "AppFile",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppFile",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
