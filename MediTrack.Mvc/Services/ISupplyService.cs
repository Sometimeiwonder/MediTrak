using MediTrack.Mvc.Models;
using MediTrack.Mvc.ViewModels;

namespace MediTrack.Mvc.Services;

public interface ISupplyService
{
    Task<List<SupplyListItemViewModel>> GetActiveSuppliesAsync();
    Task<SupplyDetailViewModel?> GetDetailAsync(int id);
    Task<SupplyStatsViewModel> GetStatsAsync();
    Task<List<MedicalSupply>> SearchAsync(string? keyword, decimal? minPrice);
    Task<List<SupplyListItemViewModel>> SearchWithStockStatusAsync(string? keyword, string? stockStatus);
    Task<MedicalSupply> CreateSupplyAsync(SupplyCreateViewModel model);
    Task<bool> UpdateSupplyAsync(SupplyEditViewModel model);
    Task<bool> SoftDeleteSupplyAsync(int id);
    Task<List<SupplyTrashItemViewModel>> GetTrashAsync();
    Task<bool> RestoreSupplyAsync(int id);
    Task<SupplyAdjustStockViewModel?> GetAdjustStockAsync(int id);
    Task<bool> AdjustStockAsync(SupplyAdjustStockViewModel model);
    Task<List<SupplyCategory>> GetCategoriesAsync();
    Task<List<SupplyListItemViewModel>> FilterSuppliesAsync(int? categoryId, decimal? minPrice, decimal? maxPrice);
    Task<List<SupplyListItemViewModel>> GetActiveSuppliesReadOnlyAsync();
    Task<SupplyDashboardViewModel> GetDashboardAsync();
}
