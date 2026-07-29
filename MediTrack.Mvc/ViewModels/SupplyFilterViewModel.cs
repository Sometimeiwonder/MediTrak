namespace MediTrack.Mvc.ViewModels;

public class SupplyFilterViewModel
{
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<MediTrack.Mvc.Models.SupplyCategory> Categories { get; set; } = new();
    public List<SupplyListItemViewModel> Results { get; set; } = new();
}
