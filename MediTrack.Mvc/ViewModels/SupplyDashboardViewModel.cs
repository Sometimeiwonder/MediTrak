namespace MediTrack.Mvc.ViewModels;

public class SupplyDashboardViewModel
{
    public int TotalSupplies { get; set; }
    public int ActiveSupplies { get; set; }
    public int DeletedSupplies { get; set; }
    public int CreatedToday { get; set; }
    public int UpdatedToday { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int AccessDeniedToday { get; set; }
    public int SensitiveActionsToday { get; set; }
    public int RejectedUploadsToday { get; set; }

    // Chart data
    public List<CategoryStockViewModel> CategoryStockData { get; set; } = new();
    public List<MonthlyActivityViewModel> MonthlyActivityData { get; set; } = new();
    public List<StockStatusViewModel> StockStatusData { get; set; } = new();
    public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
}

public class CategoryStockViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
}

public class MonthlyActivityViewModel
{
    public string Month { get; set; } = string.Empty;
    public int Created { get; set; }
    public int Updated { get; set; }
}

public class StockStatusViewModel
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentActivityViewModel
{
    public string Action { get; set; } = string.Empty;
    public string SupplyName { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}
