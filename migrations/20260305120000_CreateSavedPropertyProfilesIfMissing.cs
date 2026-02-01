using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentWisePro.Web.Migrations
{
    /// <inheritdoc />
    public partial class CreateSavedPropertyProfilesIfMissing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.SavedPropertyProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SavedPropertyProfiles (
        SavedPropertyProfileId INT IDENTITY(1,1) NOT NULL,
        InvestmentProfileId INT NOT NULL,
        RentalListingId INT NOT NULL,
        DownpaymentPercentage DECIMAL(18,4) NOT NULL,
        MortgageInterestRate DECIMAL(18,4) NOT NULL,
        TermYears INT NOT NULL,
        ClosingCostOverride DECIMAL(18,2) NULL,
        RenovationBudget DECIMAL(18,2) NULL,
        OtherUpfrontCosts DECIMAL(18,2) NULL,
        MonthlyRentOverride DECIMAL(18,2) NULL,
        MonthlyOtherExpensesOverride DECIMAL(18,2) NULL,
        SavedAtUtc DATETIME2 NOT NULL
            CONSTRAINT DF_SavedPropertyProfiles_SavedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_SavedPropertyProfiles PRIMARY KEY CLUSTERED (SavedPropertyProfileId),
        CONSTRAINT FK_SavedPropertyProfiles_InvestmentProfiles_InvestmentProfileId FOREIGN KEY (InvestmentProfileId)
            REFERENCES dbo.InvestmentProfiles (Id)
            ON DELETE NO ACTION,
        CONSTRAINT FK_SavedPropertyProfiles_RentalListings_RentalListingId FOREIGN KEY (RentalListingId)
            REFERENCES dbo.RentalListings (RentalListingId)
            ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_SavedPropertyProfiles_InvestmentProfileId
        ON dbo.SavedPropertyProfiles (InvestmentProfileId);

    CREATE NONCLUSTERED INDEX IX_SavedPropertyProfiles_RentalListingId
        ON dbo.SavedPropertyProfiles (RentalListingId);

    CREATE UNIQUE NONCLUSTERED INDEX IX_SavedPropertyProfiles_InvestmentProfileId_RentalListingId
        ON dbo.SavedPropertyProfiles (InvestmentProfileId, RentalListingId);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.SavedPropertyProfiles', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.SavedPropertyProfiles;
END
");
        }
    }
}
