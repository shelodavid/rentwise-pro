using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace RentWisePro.Etl.Persistence.Migrations;

[DbContext(typeof(RentWisePro.Etl.Persistence.Contexts.EtlDbContext))]
[Migration("20260515090000_AddGeoMarketReferenceData")]
public partial class AddGeoMarketReferenceData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_hud_fmr_GeoCode_Year",
            table: "hud_fmr");

        migrationBuilder.DropPrimaryKey(
            name: "PK_hud_fmr",
            table: "hud_fmr");

        migrationBuilder.RenameColumn(
            name: "GeoCode",
            table: "hud_fmr",
            newName: "GeoKey");

        migrationBuilder.RenameColumn(
            name: "FmrMonthlyRent",
            table: "hud_fmr",
            newName: "Fmr");

        migrationBuilder.RenameColumn(
            name: "ImportedAt",
            table: "hud_fmr",
            newName: "RetrievedAt");

        migrationBuilder.AddColumn<string>(
            name: "GeoType",
            table: "hud_fmr",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "ZIP");

        migrationBuilder.AddPrimaryKey(
            name: "PK_hud_fmr",
            table: "hud_fmr",
            columns: new[] { "GeoType", "GeoKey", "Year", "Bedrooms" });

        migrationBuilder.CreateIndex(
            name: "IX_hud_fmr_GeoType_GeoKey_Year",
            table: "hud_fmr",
            columns: new[] { "GeoType", "GeoKey", "Year" });

        migrationBuilder.CreateIndex(
            name: "IX_hud_fmr_GeoType_GeoKey_Year_Bedrooms",
            table: "hud_fmr",
            columns: new[] { "GeoType", "GeoKey", "Year", "Bedrooms" });

        migrationBuilder.CreateTable(
            name: "geo_market_stats",
            columns: table => new
            {
                GeoType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                GeoKey = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Year = table.Column<int>(type: "int", nullable: false),
                VacancyRate = table.Column<decimal>(type: "decimal(6,3)", nullable: false),
                MedianHouseholdIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "ACS"),
                RetrievedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_geo_market_stats", x => new { x.GeoType, x.GeoKey, x.Year });
            });

        migrationBuilder.CreateIndex(
            name: "IX_geo_market_stats_GeoType_GeoKey_Year",
            table: "geo_market_stats",
            columns: new[] { "GeoType", "GeoKey", "Year" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "geo_market_stats");

        migrationBuilder.DropIndex(
            name: "IX_hud_fmr_GeoType_GeoKey_Year",
            table: "hud_fmr");

        migrationBuilder.DropIndex(
            name: "IX_hud_fmr_GeoType_GeoKey_Year_Bedrooms",
            table: "hud_fmr");

        migrationBuilder.DropPrimaryKey(
            name: "PK_hud_fmr",
            table: "hud_fmr");

        migrationBuilder.DropColumn(
            name: "GeoType",
            table: "hud_fmr");

        migrationBuilder.RenameColumn(
            name: "GeoKey",
            table: "hud_fmr",
            newName: "GeoCode");

        migrationBuilder.RenameColumn(
            name: "Fmr",
            table: "hud_fmr",
            newName: "FmrMonthlyRent");

        migrationBuilder.RenameColumn(
            name: "RetrievedAt",
            table: "hud_fmr",
            newName: "ImportedAt");

        migrationBuilder.AddPrimaryKey(
            name: "PK_hud_fmr",
            table: "hud_fmr",
            columns: new[] { "Year", "GeoCode", "Bedrooms" });

        migrationBuilder.CreateIndex(
            name: "IX_hud_fmr_GeoCode_Year",
            table: "hud_fmr",
            columns: new[] { "GeoCode", "Year" });
    }
}
