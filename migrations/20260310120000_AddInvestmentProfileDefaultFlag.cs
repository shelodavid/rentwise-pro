using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentWisePro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentProfileDefaultFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "InvestmentProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE dbo.InvestmentProfiles SET IsDefault = 1 WHERE Id = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "InvestmentProfiles");
        }
    }
}
