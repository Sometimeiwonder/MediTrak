using System.ComponentModel.DataAnnotations;

namespace MediTrack.Mvc.ViewModels;

public class IssueCreateViewModel
{
    [Required(ErrorMessage = "Người nhận không được để trống")]
    public string IssuedTo { get; set; } = "";

    [Required(ErrorMessage = "Vật tư không được để trống")]
    public int SupplyId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; }

    public List<MediTrack.Mvc.Models.MedicalSupply> AvailableSupplies { get; set; } = new();
}
