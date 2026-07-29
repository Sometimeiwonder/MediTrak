using MediTrack.Mvc.Data;
using MediTrack.Mvc.Models;
using MediTrack.Mvc.Options;
using MediTrack.Mvc.Repositories;
using MediTrack.Mvc.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediTrack.Mvc.Services;

public class SupplyService : ISupplyService
{
    private readonly ISupplyRepository _repository;
    private readonly AppDbContext _context;
    private readonly AppSettings _settings;
    private readonly ILogger<SupplyService> _logger;

    public SupplyService(
        ISupplyRepository repository,
        AppDbContext context,
        IOptions<AppSettings> options,
        ILogger<SupplyService> logger)
    {
        _repository = repository;
        _context = context;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<List<SupplyListItemViewModel>> GetActiveSuppliesAsync()
    {
        var supplies = await _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return supplies.Select(MapToListItem).ToList();
    }

    public async Task<List<SupplyListItemViewModel>> GetActiveSuppliesReadOnlyAsync()
    {
        var supplies = await _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return supplies.Select(MapToListItem).ToList();
    }

    public async Task<SupplyDetailViewModel?> GetDetailAsync(int id)
    {
        var supply = await _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supply == null) return null;

        return new SupplyDetailViewModel
        {
            Id = supply.Id,
            Code = supply.Code,
            Name = supply.Name,
            Category = supply.SupplyCategory?.Name ?? "N/A",
            Supplier = supply.Supplier,
            UnitPrice = supply.UnitPrice,
            Quantity = supply.Quantity,
            MinStock = supply.MinStock,
            CreatedAt = supply.CreatedAt,
            UpdatedAt = supply.UpdatedAt,
            ImageUrl = supply.ImageUrl
        };
    }

    public async Task<SupplyStatsViewModel> GetStatsAsync()
    {
        var supplies = await _context.MediTrack.AsNoTracking().ToListAsync();
        return new SupplyStatsViewModel
        {
            TotalSupplies = supplies.Count,
            TotalQuantity = supplies.Sum(s => s.Quantity),
            TotalInventoryValue = supplies.Sum(s => s.UnitPrice * s.Quantity),
            OutOfStockCount = supplies.Count(s => s.Quantity <= 0),
            NeedReorderCount = supplies.Count(s => s.Quantity > 0 && s.Quantity <= _settings.LowStockThreshold)
        };
    }

    public async Task<List<MedicalSupply>> SearchAsync(string? keyword, decimal? minPrice)
    {
        var supplies = await _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .ToListAsync();

        var query = supplies.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(s =>
                s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (s.SupplyCategory != null && s.SupplyCategory.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                s.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (minPrice.HasValue)
            query = query.Where(s => s.UnitPrice >= minPrice.Value);

        return query.ToList();
    }

    public async Task<List<SupplyListItemViewModel>> SearchWithStockStatusAsync(string? keyword, string? stockStatus)
    {
        var supplies = await _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .ToListAsync();

        var query = supplies.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(s =>
                s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (s.SupplyCategory != null && s.SupplyCategory.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                s.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(stockStatus))
        {
            query = stockStatus.ToLower() switch
            {
                "outofstock" => query.Where(s => s.Quantity <= 0),
                "lowstock" => query.Where(s => s.Quantity > 0 && s.Quantity <= s.MinStock),
                "instock" => query.Where(s => s.Quantity > s.MinStock),
                _ => query
            };
        }

        return query.Select(MapToListItem).ToList();
    }

    public async Task<MedicalSupply> CreateSupplyAsync(SupplyCreateViewModel model)
    {
        var supply = new MedicalSupply
        {
            Code = model.Code,
            Name = model.Name,
            SupplyCategoryId = model.SupplyCategoryId,
            Supplier = model.Supplier,
            UnitPrice = model.Price,
            Quantity = model.Quantity,
            MinStock = model.MinStock,
            Description = null,
            CreatedAt = DateTime.Now,
            IsDeleted = false,
            ConcurrencyVersion = 1
        };

        await _repository.AddAsync(supply);
        await _repository.SaveChangesAsync();
        return supply;
    }

    public async Task<bool> UpdateSupplyAsync(SupplyEditViewModel model)
    {
        var supply = await _context.MediTrack.FirstOrDefaultAsync(s => s.Id == model.Id);
        if (supply == null) return false;

        if (supply.ConcurrencyVersion != model.ConcurrencyVersion)
        {
            _logger.LogWarning("Concurrency conflict when updating supply. SupplyId={SupplyId}", model.Id);
            return false;
        }

        supply.Code = model.Code;
        supply.Name = model.Name;
        supply.SupplyCategoryId = model.SupplyCategoryId;
        supply.Supplier = model.Supplier;
        supply.UnitPrice = model.Price;
        supply.Quantity = model.Quantity;
        supply.MinStock = model.MinStock;
        supply.UpdatedAt = DateTime.Now;
        supply.ConcurrencyVersion++;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cập nhật vật tư thành công. SupplyId={SupplyId}, Code={Code}", supply.Id, supply.Code);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Xung đột concurrency khi cập nhật vật tư. SupplyId={SupplyId}", model.Id);
            return false;
        }
    }

    public async Task<bool> SoftDeleteSupplyAsync(int id)
    {
        var supply = await _context.MediTrack.FirstOrDefaultAsync(s => s.Id == id);
        if (supply == null) return false;

        supply.IsDeleted = true;
        supply.DeletedAt = DateTime.Now;
        supply.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        _logger.LogWarning("Xóa mềm vật tư thành công. SupplyId={SupplyId}, Code={Code}", supply.Id, supply.Code);
        return true;
    }

    public async Task<List<SupplyTrashItemViewModel>> GetTrashAsync()
    {
        var deletedSupplies = await _context.MediTrack
            .IgnoreQueryFilters()
            .Include(s => s.SupplyCategory)
            .Where(s => s.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(s => s.DeletedAt)
            .Select(s => new SupplyTrashItemViewModel
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Category = s.SupplyCategory != null ? s.SupplyCategory.Name : "N/A",
                DeletedAt = s.DeletedAt
            })
            .ToListAsync();

        return deletedSupplies;
    }

    public async Task<bool> RestoreSupplyAsync(int id)
    {
        var supply = await _context.MediTrack
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted);

        if (supply == null) return false;

        supply.IsDeleted = false;
        supply.DeletedAt = null;
        supply.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Khôi phục vật tư thành công. SupplyId={SupplyId}, Code={Code}", supply.Id, supply.Code);
        return true;
    }

    public async Task<SupplyAdjustStockViewModel?> GetAdjustStockAsync(int id)
    {
        var supply = await _context.MediTrack
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supply == null) return null;

        return new SupplyAdjustStockViewModel
        {
            Id = supply.Id,
            Code = supply.Code,
            Name = supply.Name,
            CurrentQuantity = supply.Quantity,
            ConcurrencyVersion = supply.ConcurrencyVersion
        };
    }

    public async Task<bool> AdjustStockAsync(SupplyAdjustStockViewModel model)
    {
        var supply = await _context.MediTrack.FirstOrDefaultAsync(s => s.Id == model.Id);
        if (supply == null) return false;

        if (supply.ConcurrencyVersion != model.ConcurrencyVersion)
        {
            _logger.LogWarning("Xung đột concurrency khi điều chỉnh tồn kho. SupplyId={SupplyId}", model.Id);
            return false;
        }

        int newQuantity = supply.Quantity + model.Adjustment;
        if (newQuantity < 0) return false;

        supply.Quantity = newQuantity;
        supply.UpdatedAt = DateTime.Now;
        supply.ConcurrencyVersion++;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Điều chỉnh tồn kho thành công. SupplyId={SupplyId}, Code={Code}, Adjustment={Adjustment}, NewQuantity={NewQuantity}",
                supply.Id, supply.Code, model.Adjustment, newQuantity);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Xung đột concurrency khi điều chỉnh tồn kho. SupplyId={SupplyId}", model.Id);
            return false;
        }
    }

    public async Task<List<SupplyCategory>> GetCategoriesAsync()
        => await _repository.GetAllCategoriesAsync();

    public async Task<List<SupplyListItemViewModel>> FilterSuppliesAsync(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var supplies = await _repository.FilterAsync(categoryId, minPrice, maxPrice);
        return supplies.Select(MapToListItem).ToList();
    }

    public async Task<SupplyDashboardViewModel> GetDashboardAsync()
    {
        var supplies = await _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .ToListAsync();
        var today = DateTime.Today;

        // Category stock data for pie chart
        var categoryStockData = supplies
            .Where(s => !s.IsDeleted && s.SupplyCategory != null)
            .GroupBy(s => s.SupplyCategory!.Name)
            .Select(g => new CategoryStockViewModel
            {
                CategoryName = g.Key,
                TotalQuantity = g.Sum(s => s.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ToList();

        // Monthly activity for line chart (last 6 months)
        var monthlyActivity = new List<MonthlyActivityViewModel>();
        for (int i = 5; i >= 0; i--)
        {
            var month = today.AddMonths(-i);
            var monthSupplies = supplies.Where(s => s.CreatedAt.Month == month.Month && s.CreatedAt.Year == month.Year);
            monthlyActivity.Add(new MonthlyActivityViewModel
            {
                Month = month.ToString("MMM yyyy"),
                Created = monthSupplies.Count(s => s.CreatedAt.Date == month.Date),
                Updated = supplies.Count(s => s.UpdatedAt.HasValue && s.UpdatedAt.Value.Month == month.Month && s.UpdatedAt.Value.Year == month.Year)
            });
        }

        // Stock status for doughnut chart
        var stockStatusData = new List<StockStatusViewModel>
        {
            new() { Status = "In Stock", Count = supplies.Count(s => !s.IsDeleted && s.Quantity > s.MinStock) },
            new() { Status = "Low Stock", Count = supplies.Count(s => !s.IsDeleted && s.Quantity > 0 && s.Quantity <= s.MinStock) },
            new() { Status = "Out of Stock", Count = supplies.Count(s => !s.IsDeleted && s.Quantity <= 0) }
        };

        // Recent activities (last 5)
        var recentActivities = supplies
            .Where(s => s.UpdatedAt.HasValue)
            .OrderByDescending(s => s.UpdatedAt)
            .Take(5)
            .Select(s => new RecentActivityViewModel
            {
                Action = s.IsDeleted ? "Deleted" : "Updated",
                SupplyName = s.Name,
                Timestamp = s.UpdatedAt?.ToString("dd/MM HH:mm") ?? ""
            })
            .ToList();

        return new SupplyDashboardViewModel
        {
            TotalSupplies = supplies.Count,
            ActiveSupplies = supplies.Count(s => !s.IsDeleted),
            DeletedSupplies = supplies.Count(s => s.IsDeleted),
            CreatedToday = supplies.Count(s => s.CreatedAt.Date == today),
            UpdatedToday = supplies.Count(s => s.UpdatedAt.HasValue && s.UpdatedAt.Value.Date == today),
            LowStockCount = supplies.Count(s => !s.IsDeleted && s.Quantity > 0 && s.Quantity <= s.MinStock),
            OutOfStockCount = supplies.Count(s => !s.IsDeleted && s.Quantity <= 0),
            CategoryStockData = categoryStockData,
            MonthlyActivityData = monthlyActivity,
            StockStatusData = stockStatusData,
            RecentActivities = recentActivities
        };
    }

    private static SupplyListItemViewModel MapToListItem(MedicalSupply s) => new()
    {
        Id = s.Id,
        Code = s.Code,
        Name = s.Name,
        Category = s.SupplyCategory?.Name ?? "N/A",
        Supplier = s.Supplier,
        UnitPrice = s.UnitPrice,
        Quantity = s.Quantity,
        MinStock = s.MinStock
    };
}
