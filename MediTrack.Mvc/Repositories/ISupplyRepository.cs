using MediTrack.Mvc.Models;

namespace MediTrack.Mvc.Repositories;

public interface ISupplyRepository
{
    Task<List<SupplyCategory>> GetAllCategoriesAsync();
    Task<List<MedicalSupply>> GetAllReadOnlyAsync();
    Task<List<MedicalSupply>> GetAllWithCategoryReadOnlyAsync();
    Task<MedicalSupply?> GetByIdAsync(int id);
    Task<MedicalSupply?> GetByIdReadOnlyAsync(int id);
    Task<List<MedicalSupply>> FilterAsync(int? categoryId, decimal? minPrice, decimal? maxPrice);
    Task AddAsync(MedicalSupply supply);
    Task SaveChangesAsync();
}
