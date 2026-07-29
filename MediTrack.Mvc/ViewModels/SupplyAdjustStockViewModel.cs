using System.ComponentModel.DataAnnotations;

namespace MediTrack.Mvc.ViewModels;

public class SupplyAdjustStockViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int CurrentQuantity { get; set; }

    [Required(ErrorMessage = "Số lượng thay đổi không được để trống")]
    [Range(-100000, 100000, ErrorMessage = "Số lượng thay đổi phải từ -100000 đến 100000")]
    public int Adjustment { get; set; }

    public string RowVersion { get; set; } = string.Empty;

    public int ConcurrencyVersion { get; set; }

    public int NewQuantity => CurrentQuantity + Adjustment;

    public string CurrentQuantityText => CurrentQuantity.ToString("N0");
    public string NewQuantityText => NewQuantity.ToString("N0");
}
