using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace RentWisePro.Etl.Persistence.Migrations;

[DbContext(typeof(RentWisePro.Etl.Persistence.Contexts.EtlDbContext))]
[Migration("20260501090000_AddHudFmrRentEstimates")]
public partial class AddHudFmrRentEstimates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "EstimatedMonthlyRent",
            table: "properties",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "RentEstimateAsOf",
            table: "properties",
            type: "datetimeoffset",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RentEstimateSource",
            table: "properties",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "hud_fmr",
            columns: table => new
            {
                Year = table.Column<int>(type: "int", nullable: false),
                GeoCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Bedrooms = table.Column<int>(type: "int", nullable: false),
                FmrMonthlyRent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "HUD"),
                ImportedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_hud_fmr", x => new { x.Year, x.GeoCode, x.Bedrooms });
            });

        migrationBuilder.CreateIndex(
            name: "IX_hud_fmr_GeoCode_Year",
            table: "hud_fmr",
            columns: new[] { "GeoCode", "Year" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "hud_fmr");

        migrationBuilder.DropColumn(
            name: "EstimatedMonthlyRent",
            table: "properties");

        migrationBuilder.DropColumn(
            name: "RentEstimateAsOf",
            table: "properties");

        migrationBuilder.DropColumn(
            name: "RentEstimateSource",
            table: "properties");
    }
}
