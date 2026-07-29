namespace MediTrack.Mvc.ViewModels;

public class SupplyDeleteViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Supplier { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public string PriceText => $"{UnitPrice:N0} đ";
}
