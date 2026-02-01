namespace RentWisePro.Web.Models.Account
{
    public class EmailConfirmationPendingViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string? ConfirmationLink { get; set; }
    }
}
