using System.ComponentModel.DataAnnotations;

namespace MediTrack.Mvc.Models;

public class IssueItem
{
    public int Id { get; set; }

    public int IssueId { get; set; }
    public Issue? Issue { get; set; }

    public int MedicalSupplyId { get; set; }
    public MedicalSupply? MedicalSupply { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
