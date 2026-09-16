using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.infrastructure.Data.Migrations.TenantErpDb
{
    /// <inheritdoc />
    public partial class StreamlineCustomerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Drop obsolete columns ────────────────────────────
            migrationBuilder.DropColumn(name: "AddressLine1", table: "Customers");
            migrationBuilder.DropColumn(name: "AddressLine2", table: "Customers");
            migrationBuilder.DropColumn(name: "BirthDate", table: "Customers");
            migrationBuilder.DropColumn(name: "Gender", table: "Customers");
            migrationBuilder.DropColumn(name: "PhoneSecondary", table: "Customers");

            // ── Add new address fields ───────────────────────────
            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Village",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Street", table: "Customers");
            migrationBuilder.DropColumn(name: "Village", table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1", table: "Customers",
                type: "nvarchar(max)", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2", table: "Customers",
                type: "nvarchar(max)", nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate", table: "Customers",
                type: "datetime2", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender", table: "Customers",
                type: "nvarchar(max)", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneSecondary", table: "Customers",
                type: "nvarchar(max)", nullable: true);
        }
    }
}