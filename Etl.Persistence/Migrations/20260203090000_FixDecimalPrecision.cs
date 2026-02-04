using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentWisePro.Etl.Persistence.Migrations;

[DbContext(typeof(RentWisePro.Etl.Persistence.Contexts.EtlDbContext))]
[Migration("20260203090000_FixDecimalPrecision")]
public partial class FixDecimalPrecision : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "listings",
            type: "decimal(18,0)",
            nullable: true,
            oldClrType: typeof(decimal),
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "listing_snapshots",
            type: "decimal(18,0)",
            nullable: true,
            oldClrType: typeof(decimal),
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Beds",
            table: "properties",
            type: "decimal(4,1)",
            precision: 4,
            scale: 1,
            nullable: true,
            oldClrType: typeof(decimal),
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Baths",
            table: "properties",
            type: "decimal(4,1)",
            precision: 4,
            scale: 1,
            nullable: true,
            oldClrType: typeof(decimal),
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Latitude",
            table: "properties",
            type: "decimal(9,6)",
            precision: 9,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Longitude",
            table: "properties",
            type: "decimal(9,6)",
            precision: 9,
            scale: 6,
            nullable: true,
            oldClrType: typeof(decimal),
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "listings",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,0)",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "listing_snapshots",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,0)",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Beds",
            table: "properties",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(4,1)",
            oldPrecision: 4,
            oldScale: 1,
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Baths",
            table: "properties",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(4,1)",
            oldPrecision: 4,
            oldScale: 1,
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Latitude",
            table: "properties",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(9,6)",
            oldPrecision: 9,
            oldScale: 6,
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "Longitude",
            table: "properties",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "decimal(9,6)",
            oldPrecision: 9,
            oldScale: 6,
            oldNullable: true);
    }
}
