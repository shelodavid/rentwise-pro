using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models;
using RentWisePro.Web.Models.Identity;
using RentWisePro.Web.Services.Etl;

namespace RentWisePro.Web.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminController : Controller
{
    private readonly IEtlControlService _etlControlService;
    private readonly EtlReadDbContext _dbContext;

    public AdminController(IEtlControlService etlControlService, EtlReadDbContext dbContext)
    {
        _etlControlService = etlControlService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> EtlOps(CancellationToken cancellationToken)
    {
        var runnerStatus = await _etlControlService.GetRunnerStatusAsync(cancellationToken);
        var recentActions = await _dbContext.EtlAdminActions
            .OrderByDescending(action => action.StartedAt)
            .Take(10)
            .Select(action => new EtlAdminActionRow
            {
                ActionType = action.ActionType,
                Status = action.Status,
                StartedAt = action.StartedAt,
                FinishedAt = action.FinishedAt,
                Message = action.Message,
                RequestedByUserId = action.RequestedByUserId
            })
            .ToListAsync(cancellationToken);

        var viewModel = new EtlOpsViewModel
        {
            RunnerStatus = runnerStatus,
            RecentActions = recentActions
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunIngestionOnce(CancellationToken cancellationToken)
    {
        var result = await _etlControlService.TriggerIngestionRunOnceAsync(GetUserId(), cancellationToken);
        TempData["EtlOpsMessage"] = result.Message;
        TempData["EtlOpsSuccess"] = result.Success;
        return RedirectToAction(nameof(EtlOps));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunQueueOnce(CancellationToken cancellationToken)
    {
        var result = await _etlControlService.TriggerQueueRunOnceAsync(GetUserId(), cancellationToken);
        TempData["EtlOpsMessage"] = result.Message;
        TempData["EtlOpsSuccess"] = result.Success;
        return RedirectToAction(nameof(EtlOps));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableSchedule(CancellationToken cancellationToken)
    {
        var result = await _etlControlService.DisableLocalScheduleAsync(GetUserId(), cancellationToken);
        TempData["EtlOpsMessage"] = result.Message;
        TempData["EtlOpsSuccess"] = result.Success;
        return RedirectToAction(nameof(EtlOps));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableSchedule(CancellationToken cancellationToken)
    {
        var result = await _etlControlService.EnableLocalScheduleAsync(GetUserId(), cancellationToken);
        TempData["EtlOpsMessage"] = result.Message;
        TempData["EtlOpsSuccess"] = result.Success;
        return RedirectToAction(nameof(EtlOps));
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
