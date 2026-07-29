namespace MediTrack.Mvc.ViewModels;

public class SupplyCategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SupplyCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public string TotalInventoryValueText => $"{TotalInventoryValue:N0} d";
}
