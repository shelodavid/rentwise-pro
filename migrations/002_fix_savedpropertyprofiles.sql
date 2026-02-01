/*
RentWise Pro — SavedPropertyProfiles schema drift fix
Adds missing columns, defaults, and indexes for SavedPropertyProfiles.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

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

        ALTER TABLE dbo.SavedPropertyProfiles
            ALTER COLUMN DownpaymentPercentage DECIMAL(18,4) NOT NULL;

        ALTER TABLE dbo.SavedPropertyProfiles
            ALTER COLUMN MortgageInterestRate DECIMAL(18,4) NOT NULL;

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

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrSeverity INT = ERROR_SEVERITY();
    RAISERROR(@ErrMsg, @ErrSeverity, 1);
END CATCH
GO
