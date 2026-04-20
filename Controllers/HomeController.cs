using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TechMoveSystems.Data;
using TechMoveSystems.Models;
using TechMoveSystems.Services;
using TechMoveSystems.ViewModels;

namespace TechMoveSystems.Controllers;

public class HomeController(TechMoveDbContext context, ICurrencyService currencyService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var latestRate = await currencyService.GetLiveUsdToZarRateAsync();
        var contracts = await context.Contracts
            .Include(contract => contract.Client)
            .Include(contract => contract.ServiceRequests)
            .OrderBy(contract => contract.EndDate)
            .ToListAsync();

        var requests = await context.ServiceRequests
            .Include(request => request.Contract)
            .ThenInclude(contract => contract!.Client)
            .OrderByDescending(request => request.CreatedAt)
            .Take(5)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            ClientCount = await context.Clients.CountAsync(),
            ContractCount = contracts.Count,
            ActiveContractCount = contracts.Count(contract => contract.Status == ContractStatus.Active),
            RequestCount = await context.ServiceRequests.CountAsync(),
            LatestExchangeRate = latestRate,
            ContractsRequiringAttention = contracts
                .Where(contract => contract.Status is ContractStatus.Expired or ContractStatus.OnHold
                    || contract.EndDate <= DateTime.Today.AddDays(21))
                .Take(4)
                .ToList(),
            RecentRequests = requests
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
