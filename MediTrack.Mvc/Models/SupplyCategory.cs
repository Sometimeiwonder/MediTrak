using System.ComponentModel.DataAnnotations;

namespace MediTrack.Mvc.Models;

public class SupplyCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<MedicalSupply> Supplies { get; set; } = new List<MedicalSupply>();
}
