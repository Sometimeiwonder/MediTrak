using System.ComponentModel.DataAnnotations;

namespace MediTrack.Mvc.Models;

public class MedicalSupply
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public int SupplyCategoryId { get; set; }
    public SupplyCategory? SupplyCategory { get; set; }

    [MaxLength(100)]
    public string Supplier { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public int MinStock { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int ConcurrencyVersion { get; set; } = 1;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}
