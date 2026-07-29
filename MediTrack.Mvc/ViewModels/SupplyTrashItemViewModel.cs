namespace MediTrack.Mvc.ViewModels;

public class SupplyTrashItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTime? DeletedAt { get; set; }

    public string DeletedAtText => DeletedAt?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
}
