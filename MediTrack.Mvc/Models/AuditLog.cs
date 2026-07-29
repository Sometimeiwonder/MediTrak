using System.ComponentModel.DataAnnotations;

namespace MediTrack.Mvc.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? EntityId { get; set; }

    [MaxLength(100)]
    public string? UserName { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(50)]
    public string Result { get; set; } = "Success";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Note { get; set; }
}
