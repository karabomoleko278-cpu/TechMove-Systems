using System.ComponentModel.DataAnnotations;

namespace TechMoveSystems.Models;

public class Client
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(180)]
    [Display(Name = "Contact details")]
    public string ContactDetails { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Region { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
