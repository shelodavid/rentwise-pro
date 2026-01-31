/*
RentWise Pro — Initial Schema Migration
Target: SQL Server
Purpose: Create the baseline tables needed for:
- Investment Profiles (user assumptions)
- Rental Listings (ETL-ingested property data)
- Saved Property Listings (user-saved properties snapshot)
Notes:
- ETL implementation is out-of-scope for Phase 1; assume data is already ingested.
- This script mirrors the existing schema you provided and adds a few safe, minimal improvements:
  - Adds primary keys where missing
  - Adds basic indexes and (optional) foreign keys
  - Keeps original column types unless clearly unsafe
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* 1) InvestmentProfiles */
    IF OBJECT_ID(N'dbo.InvestmentProfiles', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[InvestmentProfiles](
            [ID] [int] IDENTITY(1,1) NOT NULL,
            [InvestmentProfileName] [varchar](255) NOT NULL,
            [DownpaymentPercentage] [decimal](18, 2) NOT NULL,
            [Term] [int] NOT NULL,
            [MortgageInterestRate] [decimal](18, 2) NOT NULL,
            [PMIRate] [decimal](18, 4) NOT NULL,
            [PropertyTaxRate] [decimal](18, 4) NOT NULL,
            [HomeownersInsurance] [decimal](18, 4) NOT NULL,
            [BalloonInsurance] [decimal](18, 4) NULL,
            [RealtorClosingFeePercentage] [decimal](18, 4) NULL,
            [ClosingCosts] [decimal](18, 4) NULL,
            [RenovationCosts] [decimal](18, 4) NULL,
            [OtherExpenses] [decimal](18, 4) NULL,
            [AnnualAppreciationRate] [decimal](18, 4) NULL,
            [VacancyRate] [decimal](18, 4) NULL,
            [PropertyManagementFee] [decimal](18, 4) NULL,
            [MonthlyMaintenanceBudget] [decimal](18, 4) NULL,
            [MonthlyUtilitiesCost] [decimal](18, 4) NULL,
            [LoanOriginationFee] [decimal](18, 2) NULL,
            [AppraisalFee] [decimal](18, 2) NULL,
            [CreditReportFee] [decimal](18, 2) NULL,
            [TitleInsuranceCost] [decimal](18, 2) NULL,
            [TitleSearchFee] [decimal](18, 2) NULL,
            [EscrowFee] [decimal](18, 2) NULL,
            [FloodInspectionFee] [decimal](18, 2) NULL,
            [MiscellaneousFees] [decimal](18, 2) NULL,
            [HOAEstimate] [decimal](18, 4) NULL,
            CONSTRAINT [PK_InvestmentProfiles] PRIMARY KEY CLUSTERED ([ID] ASC)
        ) ON [PRIMARY];
    END
    GO

    /* 2) RentalListings */
    IF OBJECT_ID(N'dbo.RentalListings', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[RentalListings](
            [ID] [int] IDENTITY(1,1) NOT NULL,
            [Zpid] [nvarchar](max) NOT NULL,
            [StreetAddress] [nvarchar](max) NOT NULL,
            [City] [nvarchar](max) NOT NULL,
            [State] [nvarchar](max) NOT NULL,
            [ZipCode] [nvarchar](max) NOT NULL,
            [PropertyType] [nvarchar](max) NOT NULL,
            [Bathrooms] [nvarchar](max) NOT NULL,
            [Bedrooms] [nvarchar](max) NOT NULL,
            [ImgSrc] [nvarchar](max) NOT NULL,
            [Price] [float] NOT NULL,
            [TaxAssessedValue] [float] NOT NULL,
            [EstimatedRent] [float] NOT NULL,
            [Latitude] [nvarchar](max) NOT NULL,
            [Longitude] [nvarchar](max) NOT NULL,
            [AnalysisDate] [datetime2](7) NOT NULL,
            [County] [varchar](255) NULL,
            CONSTRAINT [PK_RentalListings] PRIMARY KEY CLUSTERED ([ID] ASC)
        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
    END
    GO

    /* Helpful index for lookup by Zpid (since it is used as ZipID/Zpid in app flows) */
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = 'IX_RentalListings_Zpid' AND object_id = OBJECT_ID('dbo.RentalListings')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_RentalListings_Zpid]
        ON [dbo].[RentalListings] ([Zpid] ASC);
    END
    GO

    /* 3) SavedPropertyListings (existing schema provided) */
    IF OBJECT_ID(N'dbo.SavedPropertyListings', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[SavedPropertyListings](
            [UserID] [int] NOT NULL,
            [ZipID] [int] NULL,
            [StreetAddress] [nvarchar](max) NOT NULL,
            [PropertyType] [nvarchar](max) NOT NULL,
            [Bathrooms] [nvarchar](max) NOT NULL,
            [Bedrooms] [nvarchar](max) NOT NULL,
            [ImgSrc] [nvarchar](max) NOT NULL,
            [Price] [decimal](10, 2) NOT NULL,
            [TaxAssessedValue] [decimal](10, 2) NOT NULL,
            [Downpayment] [decimal](10, 2) NOT NULL,
            [EstimatedMortgageCost] [decimal](10, 2) NOT NULL,
            [EstimatedInsuranceCost] [decimal](10, 2) NOT NULL,
            [EstimatedRent] [decimal](10, 2) NOT NULL,
            [HOAEstimate] [decimal](18, 2) NULL,
            [MonthlyPMI] [decimal](10, 2) NOT NULL,
            [County] [varchar](255) NULL
        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

        /* Original default UserID value */
        ALTER TABLE [dbo].[SavedPropertyListings]
            ADD CONSTRAINT [DF_SavedPropertyListings_UserID] DEFAULT ((123456)) FOR [UserID];
    END
    GO

    /*
    Optional-but-recommended: add a surrogate key to avoid duplicate row ambiguity.
    If you want to keep the table EXACTLY as-is, comment out the block below.
    */
    IF COL_LENGTH('dbo.SavedPropertyListings', 'SavedPropertyListingID') IS NULL
    BEGIN
        ALTER TABLE dbo.SavedPropertyListings
            ADD SavedPropertyListingID INT IDENTITY(1,1) NOT NULL;

        ALTER TABLE dbo.SavedPropertyListings
            ADD CONSTRAINT PK_SavedPropertyListings PRIMARY KEY CLUSTERED (SavedPropertyListingID);
    END
    GO

    /* Useful index */
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = 'IX_SavedPropertyListings_User_ZipID' AND object_id = OBJECT_ID('dbo.SavedPropertyListings')
    )
    BEGIN
        CREATE NONCLUSTERED INDEX [IX_SavedPropertyListings_User_ZipID]
        ON [dbo].[SavedPropertyListings] ([UserID] ASC, [ZipID] ASC);
    END
    GO

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrSeverity INT = ERROR_SEVERITY();
    RAISERROR(@ErrMsg, @ErrSeverity, 1);
END CATCH
GO
