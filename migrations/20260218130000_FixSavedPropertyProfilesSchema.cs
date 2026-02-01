using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentWisePro.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixSavedPropertyProfilesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.SavedPropertyProfiles', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'SavedPropertyProfileId') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD SavedPropertyProfileId INT IDENTITY(1,1) NOT NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'InvestmentProfileId') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD InvestmentProfileId INT NOT NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'RentalListingId') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD RentalListingId INT NOT NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'DownpaymentPercentage') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD DownpaymentPercentage DECIMAL(18,4) NOT NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'MortgageInterestRate') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD MortgageInterestRate DECIMAL(18,4) NOT NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'TermYears') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD TermYears INT NOT NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'ClosingCostOverride') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD ClosingCostOverride DECIMAL(18,2) NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'RenovationBudget') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD RenovationBudget DECIMAL(18,2) NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'OtherUpfrontCosts') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD OtherUpfrontCosts DECIMAL(18,2) NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'MonthlyRentOverride') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD MonthlyRentOverride DECIMAL(18,2) NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'MonthlyOtherExpensesOverride') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD MonthlyOtherExpensesOverride DECIMAL(18,2) NULL;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'SavedAtUtc') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD SavedAtUtc DATETIME2 NOT NULL
                CONSTRAINT DF_SavedPropertyProfiles_SavedAtUtc DEFAULT (SYSUTCDATETIME());
    END

    IF EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
          AND name = 'DownpaymentPercentage'
    )
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ALTER COLUMN DownpaymentPercentage DECIMAL(18,4) NOT NULL;
    END

    IF EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
          AND name = 'MortgageInterestRate'
    )
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ALTER COLUMN MortgageInterestRate DECIMAL(18,4) NOT NULL;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.key_constraints
        WHERE parent_object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
          AND type = 'PK'
    )
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            ADD CONSTRAINT PK_SavedPropertyProfiles PRIMARY KEY CLUSTERED (SavedPropertyProfileId);
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_SavedPropertyProfiles_RentalListingId'
          AND object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_SavedPropertyProfiles_RentalListingId
            ON dbo.SavedPropertyProfiles (RentalListingId);
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_SavedPropertyProfiles_InvestmentProfileId'
          AND object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_SavedPropertyProfiles_InvestmentProfileId
            ON dbo.SavedPropertyProfiles (InvestmentProfileId);
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_SavedPropertyProfiles_InvestmentProfileId_RentalListingId'
          AND object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX IX_SavedPropertyProfiles_InvestmentProfileId_RentalListingId
            ON dbo.SavedPropertyProfiles (InvestmentProfileId, RentalListingId);
    END
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.SavedPropertyProfiles', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_SavedPropertyProfiles_InvestmentProfileId_RentalListingId'
          AND object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
    )
    BEGIN
        DROP INDEX IX_SavedPropertyProfiles_InvestmentProfileId_RentalListingId
            ON dbo.SavedPropertyProfiles;
    END

    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_SavedPropertyProfiles_InvestmentProfileId'
          AND object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
    )
    BEGIN
        DROP INDEX IX_SavedPropertyProfiles_InvestmentProfileId
            ON dbo.SavedPropertyProfiles;
    END

    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_SavedPropertyProfiles_RentalListingId'
          AND object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
    )
    BEGIN
        DROP INDEX IX_SavedPropertyProfiles_RentalListingId
            ON dbo.SavedPropertyProfiles;
    END

    IF EXISTS (
        SELECT 1
        FROM sys.key_constraints
        WHERE parent_object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
          AND name = 'PK_SavedPropertyProfiles'
    )
    BEGIN
        ALTER TABLE dbo.SavedPropertyProfiles
            DROP CONSTRAINT PK_SavedPropertyProfiles;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'SavedAtUtc') IS NOT NULL
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM sys.default_constraints dc
            JOIN sys.columns c
              ON dc.parent_object_id = c.object_id
             AND dc.parent_column_id = c.column_id
            WHERE dc.parent_object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
              AND c.name = 'SavedAtUtc'
        )
        BEGIN
            DECLARE @constraintName NVARCHAR(256);
            SELECT @constraintName = dc.name
            FROM sys.default_constraints dc
            JOIN sys.columns c
              ON dc.parent_object_id = c.object_id
             AND dc.parent_column_id = c.column_id
            WHERE dc.parent_object_id = OBJECT_ID(N'dbo.SavedPropertyProfiles')
              AND c.name = 'SavedAtUtc';

            EXEC('ALTER TABLE dbo.SavedPropertyProfiles DROP CONSTRAINT ' + @constraintName);
        END

        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN SavedAtUtc;
    END

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'MonthlyOtherExpensesOverride') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN MonthlyOtherExpensesOverride;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'MonthlyRentOverride') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN MonthlyRentOverride;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'OtherUpfrontCosts') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN OtherUpfrontCosts;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'RenovationBudget') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN RenovationBudget;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'ClosingCostOverride') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN ClosingCostOverride;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'TermYears') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN TermYears;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'MortgageInterestRate') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN MortgageInterestRate;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'DownpaymentPercentage') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN DownpaymentPercentage;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'RentalListingId') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN RentalListingId;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'InvestmentProfileId') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN InvestmentProfileId;

    IF COL_LENGTH('dbo.SavedPropertyProfiles', 'SavedPropertyProfileId') IS NOT NULL
        ALTER TABLE dbo.SavedPropertyProfiles DROP COLUMN SavedPropertyProfileId;
END
");
        }
    }
}
