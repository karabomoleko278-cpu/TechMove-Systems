using TechMoveSystems.Models;

namespace TechMoveSystems.ViewModels;

public class ContractListViewModel
{
    public List<Contract> Contracts { get; set; } = [];
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ContractStatus? Status { get; set; }
}
