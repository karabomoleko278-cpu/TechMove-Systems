using TechMoveSystems.Models;

namespace TechMoveSystems.ViewModels;

public class DashboardViewModel
{
    public int ClientCount { get; set; }
    public int ContractCount { get; set; }
    public int ActiveContractCount { get; set; }
    public int RequestCount { get; set; }
    public decimal LatestExchangeRate { get; set; }
    public IReadOnlyList<Contract> ContractsRequiringAttention { get; set; } = [];
    public IReadOnlyList<ServiceRequest> RecentRequests { get; set; } = [];
}
