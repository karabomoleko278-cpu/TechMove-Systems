using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TechMoveSystems.Models;

namespace TechMoveSystems.ViewModels;

public class ContractCreateViewModel
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Client name")]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [StringLength(180)]
    [Display(Name = "Contact details")]
    public string ContactDetails { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Region { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "Service level")]
    public string ServiceLevel { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(6);

    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required]
    [Display(Name = "Signed agreement (PDF)")]
    public IFormFile? SignedAgreement { get; set; }
}
