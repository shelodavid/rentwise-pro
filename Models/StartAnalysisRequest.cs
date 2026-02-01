using System.ComponentModel.DataAnnotations;

namespace RentWisePro.Web.Models
{
    public class StartAnalysisRequest
    {
        [Range(1, long.MaxValue)]
        public long? Zpid { get; set; }

        [Range(1, int.MaxValue)]
        public int? RentalListingId { get; set; }
    }
}
