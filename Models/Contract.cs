using System.ComponentModel.DataAnnotations;

namespace TechMoveSystems.Models;

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    OnHold
}

public class Contract
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    [Display(Name = "Service level")]
    public string ServiceLevel { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime EndDate { get; set; }

    public ContractStatus Status { get; set; }

    [Display(Name = "Signed agreement path")]
    public string SignedAgreementPath { get; set; } = string.Empty;

    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
