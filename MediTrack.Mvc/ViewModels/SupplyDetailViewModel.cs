namespace MediTrack.Mvc.ViewModels;

public class SupplyDetailViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Supplier { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ImageUrl { get; set; }

    public string PriceText => $"{UnitPrice:N0} đ";
    public decimal InventoryValue => UnitPrice * Quantity;
    public string InventoryValueText => $"{InventoryValue:N0} đ";
    public string CreatedAtText => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public string UpdatedAtText => UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa cập nhật";

    public string StockStatus
    {
        get
        {
            if (Quantity <= 0) return "Hết hàng";
            if (Quantity <= MinStock) return "Cần nhập thêm";
            return "Còn hàng";
        }
    }

    public string ReorderSuggestion
    {
        get
        {
            if (Quantity <= 0)
                return "Cần nhập vật tư ngay vì đã hết hàng.";
            if (Quantity <= MinStock)
                return $"Nên nhập thêm. Tồn kho hiện tại chỉ còn {Quantity}, mức tối thiểu là {MinStock}.";
            return "Tồn kho đang ổn định, chưa cần nhập thêm.";
        }
    }
}