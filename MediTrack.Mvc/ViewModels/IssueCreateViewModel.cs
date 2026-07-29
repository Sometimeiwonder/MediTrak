using System.ComponentModel.DataAnnotations;

namespace MediTrack.Mvc.ViewModels;

public class IssueCreateViewModel
{
    [Required(ErrorMessage = "Nguoi nhan khong duoc de trong")]
    public string IssuedTo { get; set; } = "";

    [Required(ErrorMessage = "Vat tu khong duoc de trong")]
    public int SupplyId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "So luong phai lon hon 0")]
    public int Quantity { get; set; }

    public List<MediTrack.Mvc.Models.MedicalSupply> AvailableSupplies { get; set; } = new();
}
