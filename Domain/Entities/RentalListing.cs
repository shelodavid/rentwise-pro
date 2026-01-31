using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentWisePro.Web.Domain.Entities
{
    [Table("RentalListings")]
    public class RentalListing
    {
        [Key]
        public int RentalListingId { get; set; } // internal PK (ETL-friendly)

        // External source identity (Zillow etc.)
        [Required]
        public long Zpid { get; set; }

        [MaxLength(255)]
        public string? StreetAddress { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(2)]
        public string? State { get; set; }

        [MaxLength(10)]
        public string? ZipCode { get; set; }

        [MaxLength(100)]
        public string? County { get; set; }

        [MaxLength(50)]
        public string? PropertyType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedRent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TaxAssessedValue { get; set; }

        [MaxLength(1000)]
        public string? ImgSrc { get; set; }

        public int? Bedrooms { get; set; }
        public decimal? Bathrooms { get; set; }

        // ETL metadata
        [MaxLength(50)]
        public string? SourceSystem { get; set; } // "Zillow", "HUD", etc.

        public DateTime IngestedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<SavedPropertyProfile> SavedPropertyProfiles { get; set; } = new List<SavedPropertyProfile>();
    }
}
