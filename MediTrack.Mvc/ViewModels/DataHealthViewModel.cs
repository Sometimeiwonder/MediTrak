namespace MediTrack.Mvc.ViewModels;

public class DataHealthViewModel
{
    public bool DatabaseExists { get; set; }
    public int MigrationCount { get; set; }
    public string LastMigrationName { get; set; } = string.Empty;
    public int TotalCategories { get; set; }
    public int TotalSupplies { get; set; }
    public int TotalIssues { get; set; }
    public int TotalIssueItems { get; set; }
    public List<string> SeedStatus { get; set; } = new();
    public string TrackingDemoResult { get; set; } = string.Empty;
}
