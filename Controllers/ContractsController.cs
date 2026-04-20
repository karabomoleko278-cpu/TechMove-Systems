using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMoveSystems.Data;
using TechMoveSystems.Models;
using TechMoveSystems.Services;
using TechMoveSystems.ViewModels;

namespace TechMoveSystems.Controllers;

public class ContractsController(
    TechMoveDbContext context,
    IFileStorageService fileStorageService) : Controller
{
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, ContractStatus? status)
    {
        var query = context.Contracts
            .Include(contract => contract.Client)
            .Include(contract => contract.ServiceRequests)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(contract => contract.StartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(contract => contract.EndDate <= endDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(contract => contract.Status == status.Value);
        }

        var model = new ContractListViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            Contracts = await query
                .OrderBy(contract => contract.EndDate)
                .ToListAsync()
        };

        return View(model);
    }

    public IActionResult Create()
    {
        return View(new ContractCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractCreateViewModel model, CancellationToken cancellationToken)
    {
        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be later than the start date.");
        }

        if (!fileStorageService.IsValidPdf(model.SignedAgreement))
        {
            ModelState.AddModelError(nameof(model.SignedAgreement), "Upload a non-empty PDF agreement.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var storedPath = await fileStorageService.SaveAgreementAsync(model.SignedAgreement!, cancellationToken);

        var client = new Client
        {
            Name = model.ClientName,
            ContactDetails = model.ContactDetails,
            Region = model.Region
        };

        var contract = new Contract
        {
            Client = client,
            ServiceLevel = model.ServiceLevel,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Status = model.Status,
            SignedAgreementPath = storedPath
        };

        context.Contracts.Add(contract);
        await context.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Contract and signed agreement were saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await context.Contracts.FindAsync(id);
        if (contract is null || string.IsNullOrWhiteSpace(contract.SignedAgreementPath))
        {
            return NotFound();
        }

        var absolutePath = fileStorageService.GetAbsolutePath(contract.SignedAgreementPath);
        if (!System.IO.File.Exists(absolutePath))
        {
            return NotFound();
        }

        var fileName = $"contract-{contract.Id}-agreement.pdf";
        return PhysicalFile(absolutePath, "application/pdf", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var contract = await context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (contract is not null)
        {
            // Remove the stored PDF file if one exists.
            if (!string.IsNullOrWhiteSpace(contract.SignedAgreementPath))
            {
                var abs = fileStorageService.GetAbsolutePath(contract.SignedAgreementPath);
                if (System.IO.File.Exists(abs))
                {
                    System.IO.File.Delete(abs);
                }
            }

            context.Contracts.Remove(contract);

            // Also remove the client if it has no other contracts.
            if (contract.Client is not null)
            {
                var hasOtherContracts = await context.Contracts
                    .AnyAsync(c => c.ClientId == contract.Client.Id && c.Id != contract.Id, cancellationToken);
                if (!hasOtherContracts)
                {
                    context.Clients.Remove(contract.Client);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            TempData["SuccessMessage"] = "Contract deleted successfully.";
        }
        return RedirectToAction(nameof(Index));
    }
}
