using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveSystems.Models;

namespace TechMoveSystems.ViewModels;

public class ServiceRequestCreateViewModel
{
    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }

    [Required]
    [StringLength(240)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    [Display(Name = "Estimated cost (USD)")]
    public decimal CostUsd { get; set; }

    public decimal CostZar { get; set; }
    public decimal ExchangeRate { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public List<SelectListItem> Contracts { get; set; } = [];
    public string WorkflowNotice { get; set; } = string.Empty;
    public bool HasAvailableContracts => Contracts.Count > 0;
}
