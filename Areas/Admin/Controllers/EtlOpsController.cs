using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentWisePro.Web.Models.Identity;
using RentWisePro.Web.Services;

namespace RentWisePro.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class EtlOpsController : Controller
{
    private readonly EtlOpsMetricsService _metricsService;

    public EtlOpsController(EtlOpsMetricsService metricsService)
    {
        _metricsService = metricsService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = await _metricsService.GetDashboardAsync(cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> RunDetails(Guid runId, CancellationToken cancellationToken)
    {
        var viewModel = await _metricsService.GetRunDetailsAsync(runId, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }

        return View(viewModel);
    }
}
