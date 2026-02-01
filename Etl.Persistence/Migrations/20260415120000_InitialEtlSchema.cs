using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentWisePro.Etl.Persistence.Migrations;

public partial class InitialEtlSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "etl_runs",
            columns: table => new
            {
                RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                FinishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_etl_runs", x => x.RunId));

        migrationBuilder.CreateTable(
            name: "properties",
            columns: table => new
            {
                PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                NormalizedAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                NormalizedAddressHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                OriginalAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Street = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Zip = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                PropertyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                YearBuilt = table.Column<int>(type: "int", nullable: true),
                SquareFeet = table.Column<int>(type: "int", nullable: true),
                Beds = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                Baths = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                NormalizationVersion = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_properties", x => x.PropertyId));

        migrationBuilder.CreateTable(
            name: "raw_payload_refs",
            columns: table => new
            {
                RawRef = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SourceListingId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                FetchedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_raw_payload_refs", x => x.RawRef));

        migrationBuilder.CreateTable(
            name: "etl_run_source_stats",
            columns: table => new
            {
                RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ListingsFetched = table.Column<int>(type: "int", nullable: false),
                ListingsUpserted = table.Column<int>(type: "int", nullable: false),
                SnapshotsCreated = table.Column<int>(type: "int", nullable: false),
                RawPayloadsSaved = table.Column<int>(type: "int", nullable: false),
                Errors = table.Column<int>(type: "int", nullable: false),
                DurationMs = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_etl_run_source_stats", x => new { x.RunId, x.Source }));

        migrationBuilder.CreateTable(
            name: "listings",
            columns: table => new
            {
                ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SourceListingId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                FirstSeenAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastSeenAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                SoldAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                MaterialHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                MissingRuns = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_listings", x => x.ListingId);
                table.ForeignKey(
                    name: "FK_listings_properties_PropertyId",
                    column: x => x.PropertyId,
                    principalTable: "properties",
                    principalColumn: "PropertyId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "property_photos",
            columns: table => new
            {
                PhotoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PhotoIndex = table.Column<int>(type: "int", nullable: false),
                UrlOriginal = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Checksum = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                Width = table.Column<int>(type: "int", nullable: true),
                Height = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_property_photos", x => x.PhotoId);
                table.ForeignKey(
                    name: "FK_property_photos_properties_PropertyId",
                    column: x => x.PropertyId,
                    principalTable: "properties",
                    principalColumn: "PropertyId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "work_queue",
            columns: table => new
            {
                WorkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WorkType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Attempts = table.Column<int>(type: "int", nullable: false),
                AvailableAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_work_queue", x => x.WorkId));

        migrationBuilder.CreateTable(
            name: "listing_snapshots",
            columns: table => new
            {
                SnapshotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ListingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                MaterialHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                ScrapedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                RawRef = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_listing_snapshots", x => x.SnapshotId);
                table.ForeignKey(
                    name: "FK_listing_snapshots_listings_ListingId",
                    column: x => x.ListingId,
                    principalTable: "listings",
                    principalColumn: "ListingId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_listing_snapshots_ListingId_ScrapedAt",
            table: "listing_snapshots",
            columns: new[] { "ListingId", "ScrapedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_listings_PropertyId",
            table: "listings",
            column: "PropertyId");

        migrationBuilder.CreateIndex(
            name: "IX_listings_Source_SourceListingId",
            table: "listings",
            columns: new[] { "Source", "SourceListingId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_listings_Status",
            table: "listings",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_listings_LastSeenAt",
            table: "listings",
            column: "LastSeenAt");

        migrationBuilder.CreateIndex(
            name: "IX_properties_NormalizedAddressHash",
            table: "properties",
            column: "NormalizedAddressHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_property_photos_PropertyId_Source_PhotoIndex",
            table: "property_photos",
            columns: new[] { "PropertyId", "Source", "PhotoIndex" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_work_queue_Status_AvailableAt",
            table: "work_queue",
            columns: new[] { "Status", "AvailableAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "listing_snapshots");
        migrationBuilder.DropTable(name: "property_photos");
        migrationBuilder.DropTable(name: "raw_payload_refs");
        migrationBuilder.DropTable(name: "work_queue");
        migrationBuilder.DropTable(name: "etl_run_source_stats");
        migrationBuilder.DropTable(name: "listings");
        migrationBuilder.DropTable(name: "etl_runs");
        migrationBuilder.DropTable(name: "properties");
    }
}
