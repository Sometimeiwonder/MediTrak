namespace MediTrack.Mvc.ViewModels;

public class IssueListItemViewModel
{
    public int Id { get; set; }
    public string IssuedTo { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string TotalAmountText => $"{TotalAmount:N0} d";
    public string IssuedAtText => IssuedAt.ToString("dd/MM/yyyy HH:mm");
    public int ItemCount { get; set; }
}
