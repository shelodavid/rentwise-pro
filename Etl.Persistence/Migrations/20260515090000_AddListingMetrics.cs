using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentWisePro.Etl.Persistence.Migrations;

[DbContext(typeof(RentWisePro.Etl.Persistence.Contexts.EtlDbContext))]
[Migration("20260515090000_AddListingMetrics")]
public partial class AddListingMetrics : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "listings",
            type: "decimal(18,2)",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,0)",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "listing_snapshots",
            type: "decimal(18,2)",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,0)",
            oldNullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "AffordabilityIndex",
            table: "listings",
            type: "decimal(18,6)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "EstimatedCashFlow",
            table: "listings",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "EstimatedRent",
            table: "listings",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "Grm",
            table: "listings",
            type: "decimal(18,6)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "PricePerSqft",
            table: "listings",
            type: "decimal(18,2)",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "RprMonthly",
            table: "listings",
            type: "decimal(18,6)",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "listing_metric_snapshots",
            columns: table => new
            {
                MetricSnapshotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AsOf = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                EstimatedRent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                RprMonthly = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                Grm = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                EstimatedCashFlow = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                AffordabilityIndex = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                FmrUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                VacancyRateUsed = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                Score = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                ScoreVersion = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_listing_metric_snapshots", x => x.MetricSnapshotId);
                table.ForeignKey(
                    name: "FK_listing_metric_snapshots_listings_ListingId",
                    column: x => x.ListingId,
                    principalTable: "listings",
                    principalColumn: "ListingId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_listing_metric_snapshots_ListingId_AsOf",
            table: "listing_metric_snapshots",
            columns: new[] { "ListingId", "AsOf" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "listing_metric_snapshots");

        migrationBuilder.DropColumn(
            name: "AffordabilityIndex",
            table: "listings");

        migrationBuilder.DropColumn(
            name: "EstimatedCashFlow",
            table: "listings");

        migrationBuilder.DropColumn(
            name: "EstimatedRent",
            table: "listings");

        migrationBuilder.DropColumn(
            name: "Grm",
            table: "listings");

        migrationBuilder.DropColumn(
            name: "PricePerSqft",
            table: "listings");

        migrationBuilder.DropColumn(
            name: "RprMonthly",
            table: "listings");

        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "listings",
            type: "decimal(18,0)",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "listing_snapshots",
            type: "decimal(18,0)",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,2)",
            oldNullable: true);
    }
}
