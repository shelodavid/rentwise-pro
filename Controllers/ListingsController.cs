using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace RentWisePro.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class ListingsController : Controller
{
    [HttpGet]
    public IActionResult Index(
        string? status,
        string? source,
        string? city,
        string? state,
        string? sortBy)
    {
        return RedirectToAction(
            "Index",
            "EtlOps",
            new
            {
                area = "Admin",
                status,
                source,
                city,
                state,
                sortBy
            });
    }
}
