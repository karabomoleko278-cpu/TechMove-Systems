using TechMoveSystems.Models;

namespace TechMoveSystems.Services;

public interface IWorkflowService
{
    bool CanCreateRequest(Contract? contract);
    string? GetCreateRequestError(Contract? contract);
}

public class WorkflowService : IWorkflowService
{
    public bool CanCreateRequest(Contract? contract) => GetCreateRequestError(contract) is null;

    public string? GetCreateRequestError(Contract? contract)
    {
        if (contract is null)
        {
            return "The selected contract could not be found.";
        }

        if (contract.Status is ContractStatus.Expired or ContractStatus.OnHold)
        {
            return "Service requests cannot be created for expired or on-hold contracts.";
        }

        if (contract.EndDate.Date < DateTime.Today)
        {
            return "The selected contract has already reached its end date.";
        }

        return null;
    }
}
