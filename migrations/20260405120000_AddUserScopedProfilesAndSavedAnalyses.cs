using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentWisePro.Web.Migrations
{
    public partial class AddUserScopedProfilesAndSavedAnalyses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE InvestmentProfiles
SET UserId = N'SYSTEM'
WHERE UserId IS NULL OR LTRIM(RTRIM(UserId)) = '';
");

            migrationBuilder.Sql(@"
WITH SystemProfiles AS (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS RowNumber
    FROM InvestmentProfiles
    WHERE UserId = N'SYSTEM'
)
UPDATE InvestmentProfiles
SET IsDefault = CASE WHEN SystemProfiles.RowNumber = 1 THEN 1 ELSE 0 END
FROM InvestmentProfiles
INNER JOIN SystemProfiles ON InvestmentProfiles.Id = SystemProfiles.Id;
");

            migrationBuilder.Sql(@"
UPDATE SavedPropertyProfiles
SET UserId = N'SYSTEM'
WHERE UserId IS NULL OR LTRIM(RTRIM(UserId)) = '';
");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "InvestmentProfiles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SavedPropertyProfiles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentProfiles_UserId_IsDefault",
                table: "InvestmentProfiles",
                columns: new[] { "UserId", "IsDefault" },
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SavedPropertyProfiles_UserId_SavedAtUtc",
                table: "SavedPropertyProfiles",
                columns: new[] { "UserId", "SavedAtUtc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvestmentProfiles_UserId_IsDefault",
                table: "InvestmentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_SavedPropertyProfiles_UserId_SavedAtUtc",
                table: "SavedPropertyProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "InvestmentProfiles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SavedPropertyProfiles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);
        }
    }
}
