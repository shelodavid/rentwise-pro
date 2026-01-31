using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentWisePro.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialDomainFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestmentProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvestmentProfileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DownpaymentPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermYears = table.Column<int>(type: "int", nullable: false),
                    MortgageInterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PMIRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PropertyTaxRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    HomeownersInsuranceAnnual = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    VacancyRate = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PropertyManagementFeePct = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MonthlyMaintenanceBudget = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MonthlyUtilitiesCost = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    RealtorClosingFeePercentage = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ClosingCostsPercentage = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LoanOriginationFeePct = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AppraisalFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreditReportFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TitleInsuranceCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TitleSearchFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EscrowFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FloodInspectionFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MiscellaneousFees = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HOAEstimate = table.Column<decimal>(type: "decimal(18,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RentalListings",
                columns: table => new
                {
                    RentalListingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Zpid = table.Column<long>(type: "bigint", nullable: false),
                    StreetAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    County = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PropertyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EstimatedRent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TaxAssessedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ImgSrc = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Bedrooms = table.Column<int>(type: "int", nullable: true),
                    Bathrooms = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IngestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalListings", x => x.RentalListingId);
                });

            migrationBuilder.CreateTable(
                name: "SavedPropertyProfiles",
                columns: table => new
                {
                    SavedPropertyProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvestmentProfileId = table.Column<int>(type: "int", nullable: false),
                    RentalListingId = table.Column<int>(type: "int", nullable: false),
                    DownpaymentPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MortgageInterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermYears = table.Column<int>(type: "int", nullable: false),
                    ClosingCostOverride = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RenovationBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OtherUpfrontCosts = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyRentOverride = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MonthlyOtherExpensesOverride = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SavedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedPropertyProfiles", x => x.SavedPropertyProfileId);
                    table.ForeignKey(
                        name: "FK_SavedPropertyProfiles_InvestmentProfiles_InvestmentProfileId",
                        column: x => x.InvestmentProfileId,
                        principalTable: "InvestmentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavedPropertyProfiles_RentalListings_RentalListingId",
                        column: x => x.RentalListingId,
                        principalTable: "RentalListings",
                        principalColumn: "RentalListingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "InvestmentProfiles",
                columns: new[] { "Id", "AppraisalFee", "ClosingCostsPercentage", "CreditReportFee", "DownpaymentPercentage", "EscrowFee", "FloodInspectionFee", "HOAEstimate", "HomeownersInsuranceAnnual", "InvestmentProfileName", "LoanOriginationFeePct", "MiscellaneousFees", "MonthlyMaintenanceBudget", "MonthlyUtilitiesCost", "MortgageInterestRate", "PMIRate", "PropertyManagementFeePct", "PropertyTaxRate", "RealtorClosingFeePercentage", "TermYears", "TitleInsuranceCost", "TitleSearchFee", "VacancyRate" },
                values: new object[] { 1, null, null, null, 20m, null, null, null, 0m, "Default", null, null, null, null, 6.50m, 0m, null, 0m, null, 30, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_RentalListings_Zpid",
                table: "RentalListings",
                column: "Zpid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedPropertyProfiles_InvestmentProfileId_RentalListingId",
                table: "SavedPropertyProfiles",
                columns: new[] { "InvestmentProfileId", "RentalListingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedPropertyProfiles_RentalListingId",
                table: "SavedPropertyProfiles",
                column: "RentalListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedPropertyProfiles");

            migrationBuilder.DropTable(
                name: "InvestmentProfiles");

            migrationBuilder.DropTable(
                name: "RentalListings");
        }
    }
}
