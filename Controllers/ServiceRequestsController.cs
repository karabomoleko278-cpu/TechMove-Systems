using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TechMoveSystems.Data;
using TechMoveSystems.Models;
using TechMoveSystems.Services;
using TechMoveSystems.ViewModels;

namespace TechMoveSystems.Controllers;

public class ServiceRequestsController(
    TechMoveDbContext context,
    ICurrencyService currencyService,
    ICurrencyCalculator currencyCalculator,
    IWorkflowService workflowService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var requests = await context.ServiceRequests
            .Include(request => request.Contract)
            .ThenInclude(contract => contract!.Client)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();

        return View(requests);
    }

    public async Task<IActionResult> Create()
    {
        var model = await BuildCreateViewModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel model, CancellationToken cancellationToken)
    {
        // ── Culture-safe decimal parsing ────────────────────────────────────
        // The SA locale uses commas as decimal separators. ASP.NET's model
        // binder can fail to parse fields like "200,50" → clears the binding
        // error and re-parses from raw form values using InvariantCulture.
        static decimal ParseDecimal(string? raw) =>
            decimal.TryParse(
                (raw ?? "0").Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var v) ? v : 0m;

        foreach (var key in new[] { nameof(model.CostUsd), nameof(model.ExchangeRate), nameof(model.CostZar) })
        {
            ModelState.Remove(key);
        }
        model.CostUsd      = ParseDecimal(Request.Form[nameof(model.CostUsd)]);
        model.ExchangeRate = ParseDecimal(Request.Form[nameof(model.ExchangeRate)]);
        model.CostZar      = ParseDecimal(Request.Form[nameof(model.CostZar)]);
        // ───────────────────────────────────────────────────────────────────

        await PopulateContractsAsync(model, cancellationToken);

        var contract = await context.Contracts
            .Include(item => item.Client)
            .FirstOrDefaultAsync(item => item.Id == model.ContractId, cancellationToken);

        var workflowError = workflowService.GetCreateRequestError(contract);
        if (workflowError is not null)
        {
            ModelState.AddModelError(nameof(model.ContractId), workflowError);
        }

        if (model.ExchangeRate <= 0)
        {
            model.ExchangeRate = await currencyService.GetLiveUsdToZarRateAsync(cancellationToken);
        }

        // Always recalculate ZAR server-side so it is never stale or zero.
        if (model.CostUsd >= 0 && model.ExchangeRate > 0)
        {
            model.CostZar = currencyCalculator.ConvertUsdToZar(model.CostUsd, model.ExchangeRate);
        }

        if (ModelState.IsValid)
        {
            var request = new ServiceRequest
            {
                ContractId = model.ContractId,
                Description = model.Description,
                CostUsd = model.CostUsd,
                CostZar = model.CostZar,
                ExchangeRate = model.ExchangeRate,
                Status = model.Status,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                context.ServiceRequests.Add(request);
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (SqlException)
            {
                ModelState.AddModelError(string.Empty, "The SQL Server connection is not available right now. Verify the local SQL Server LocalDB instance, then try again.");
                return View(model);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "The service request could not be saved to the database. Please try again after checking the SQL Server connection.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Service request saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    private async Task<ServiceRequestCreateViewModel> BuildCreateViewModelAsync(CancellationToken cancellationToken = default)
    {
        var model = new ServiceRequestCreateViewModel
        {
            ExchangeRate = await currencyService.GetLiveUsdToZarRateAsync(cancellationToken)
        };

        await PopulateContractsAsync(model, cancellationToken);
        return model;
    }

    private async Task PopulateContractsAsync(ServiceRequestCreateViewModel model, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;

        model.Contracts = await context.Contracts
            .Include(contract => contract.Client)
            .Where(contract =>
                contract.Status != ContractStatus.Expired &&
                contract.Status != ContractStatus.OnHold &&
                contract.EndDate.Date >= today)
            .OrderBy(contract => contract.Client!.Name)
            .Select(contract => new SelectListItem
            {
                Value = contract.Id.ToString(),
                Text = $"{contract.Client!.Name} | {contract.ServiceLevel} | {contract.Status}"
            })
            .ToListAsync(cancellationToken);

        model.WorkflowNotice = model.HasAvailableContracts
            ? "Only active workflow-ready contracts are available for selection."
            : "No eligible contracts are available yet. Create an active contract first, then return to this page.";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var request = await context.ServiceRequests.FindAsync([id], cancellationToken);
        if (request is not null)
        {
            context.ServiceRequests.Remove(request);
            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Service request deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
