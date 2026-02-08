using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using RentWisePro.Web.Data;

#nullable disable

namespace RentWisePro.Web.Migrations
{
    [DbContext(typeof(RentWiseProDbContext))]
    [Migration("20260521120000_AddRentalListingMetricsColumns")]
    public partial class AddRentalListingMetricsColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RentalListings', 'Rpr') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RentalListings] ADD [Rpr] decimal(18,6) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RentalListings', 'Grm') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RentalListings] ADD [Grm] decimal(18,4) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RentalListings', 'CashFlow') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RentalListings] ADD [CashFlow] decimal(18,2) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RentalListings', 'PricePerSqft') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RentalListings] ADD [PricePerSqft] decimal(18,2) NULL;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RentalListings', 'PricePerSqft') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RentalListings] DROP COLUMN [PricePerSqft];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RentalListings', 'CashFlow') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RentalListings] DROP COLUMN [CashFlow];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RentalListings', 'Grm') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RentalListings] DROP COLUMN [Grm];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RentalListings', 'Rpr') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RentalListings] DROP COLUMN [Rpr];
                END
                """);
        }
    }
}
