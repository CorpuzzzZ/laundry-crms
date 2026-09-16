using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.infrastructure.Data.Migrations.MasterDb
{
    public partial class StreamlineBranchFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AddressLine1", table: "Branches");
            migrationBuilder.DropColumn(name: "AddressLine2", table: "Branches");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Branches",
                newName: "Province");

            migrationBuilder.AddColumn<string>(
                name: "Street", table: "Branches",
                type: "nvarchar(200)", maxLength: 200,
                nullable: false, defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Village", table: "Branches",
                type: "nvarchar(200)", maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Province", table: "Branches",
                type: "nvarchar(max)", nullable: true,
                oldClrType: typeof(string), oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Street", table: "Branches");
            migrationBuilder.DropColumn(name: "Village", table: "Branches");

            migrationBuilder.RenameColumn(
                name: "Province",
                table: "Branches",
                newName: "State");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1", table: "Branches",
                type: "nvarchar(max)", nullable: false, defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2", table: "Branches",
                type: "nvarchar(max)", nullable: true);
        }
    }
}