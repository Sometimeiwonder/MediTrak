namespace MediTrack.Mvc.ViewModels;

public class SupplySearchViewModel
{
    public string Keyword { get; set; } = "";
    public decimal? MinPrice { get; set; }
    public string StockStatus { get; set; } = "";
    public List<SupplyListItemViewModel> Supplies { get; set; } = new();
}