using System.ComponentModel.DataAnnotations;

namespace MediTrack.Mvc.Models;

public class Issue
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string IssuedTo { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; } = DateTime.Now;

    public decimal TotalAmount { get; set; }

    public ICollection<IssueItem> IssueItems { get; set; } = new List<IssueItem>();
}
