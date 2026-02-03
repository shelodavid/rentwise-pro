using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace RentWisePro.Etl.Persistence.Migrations;

[DbContext(typeof(RentWisePro.Etl.Persistence.Contexts.EtlDbContext))]
[Migration("20260420090000_AddRentForecasts")]
public partial class AddRentForecasts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "rent_forecasts",
            columns: table => new
            {
                ForecastId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                EstimatedRent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                IsStub = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_rent_forecasts", x => x.ForecastId));

        migrationBuilder.CreateIndex(
            name: "IX_rent_forecasts_PropertyId_Source",
            table: "rent_forecasts",
            columns: new[] { "PropertyId", "Source" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "rent_forecasts");
    }
}
