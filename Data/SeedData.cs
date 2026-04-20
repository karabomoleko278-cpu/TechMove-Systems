using Microsoft.EntityFrameworkCore;
using TechMoveSystems.Models;

namespace TechMoveSystems.Data;

public static class SeedData
{
    public static async Task InitializeAsync(TechMoveDbContext context)
    {
        if (await context.Clients.AnyAsync())
        {
            return;
        }

        var clientA = new Client
        {
            Name = "NorthStar Logistics",
            ContactDetails = "ops@northstarlogistics.co.za",
            Region = "Johannesburg"
        };

        var clientB = new Client
        {
            Name = "BluePeak Imports",
            ContactDetails = "supply@bluepeak.co.za",
            Region = "Cape Town"
        };

        var activeContract = new Contract
        {
            Client = clientA,
            ServiceLevel = "Premium Freight Support",
            StartDate = DateTime.Today.AddDays(-20),
            EndDate = DateTime.Today.AddMonths(6),
            Status = ContractStatus.Active,
            SignedAgreementPath = string.Empty
        };

        var expiredContract = new Contract
        {
            Client = clientB,
            ServiceLevel = "Standard Cross-Border Services",
            StartDate = DateTime.Today.AddMonths(-8),
            EndDate = DateTime.Today.AddDays(-10),
            Status = ContractStatus.Expired,
            SignedAgreementPath = string.Empty
        };

        context.Clients.AddRange(clientA, clientB);
        context.Contracts.AddRange(activeContract, expiredContract);
        context.ServiceRequests.Add(new ServiceRequest
        {
            Contract = activeContract,
            Description = "Consolidate inbound hardware shipment from New Jersey.",
            Status = RequestStatus.Pending,
            CostUsd = 450.00m,
            CostZar = 8550.00m,
            ExchangeRate = 19.0m,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });

        await context.SaveChangesAsync();
    }
}
