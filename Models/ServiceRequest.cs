using System.ComponentModel.DataAnnotations;

namespace TechMoveSystems.Models;

public enum RequestStatus
{
    Pending,
    InProgress,
    Completed,
    Blocked
}

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    [StringLength(240)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    [Display(Name = "Cost (USD)")]
    public decimal CostUsd { get; set; }

    [Range(0, 50_000_000)]
    [Display(Name = "Cost (ZAR)")]
    public decimal CostZar { get; set; }

    [Range(0.0001, 1000)]
    [Display(Name = "Exchange rate")]
    public decimal ExchangeRate { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    [Display(Name = "Created at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ContractId { get; set; }
    public Contract? Contract { get; set; }
}
