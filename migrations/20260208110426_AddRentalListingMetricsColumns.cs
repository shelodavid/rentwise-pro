using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentWisePro.Web.migrations
{
    /// <inheritdoc />
    public partial class AddRentalListingMetricsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ONLY Web schema changes belong here.
            // ETL tables (etl_runs, listings, properties, etc.) are owned by EtlDbContext migrations.

            migrationBuilder.AddColumn<decimal>(
                name: "CashFlow",
                table: "RentalListings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Grm",
                table: "RentalListings",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerSqft",
                table: "RentalListings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rpr",
                table: "RentalListings",
                type: "decimal(18,6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashFlow",
                table: "RentalListings");

            migrationBuilder.DropColumn(
                name: "Grm",
                table: "RentalListings");

            migrationBuilder.DropColumn(
                name: "PricePerSqft",
                table: "RentalListings");

            migrationBuilder.DropColumn(
                name: "Rpr",
                table: "RentalListings");
        }
    }
}
