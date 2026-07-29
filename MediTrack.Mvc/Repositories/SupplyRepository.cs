using MediTrack.Mvc.Data;
using MediTrack.Mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Repositories;

public class SupplyRepository : ISupplyRepository
{
    private readonly AppDbContext _context;

    public SupplyRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<SupplyCategory>> GetAllCategoriesAsync()
        => _context.SupplyCategories.ToListAsync();

    public Task<List<MedicalSupply>> GetAllReadOnlyAsync()
        => _context.MediTrack.AsNoTracking().ToListAsync();

    public Task<List<MedicalSupply>> GetAllWithCategoryReadOnlyAsync()
        => _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .ToListAsync();

    public Task<MedicalSupply?> GetByIdAsync(int id)
        => _context.MediTrack
            .Include(s => s.SupplyCategory)
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<MedicalSupply?> GetByIdReadOnlyAsync(int id)
        => _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<MedicalSupply>> FilterAsync(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var query = _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(s => s.SupplyCategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(s => s.UnitPrice >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(s => s.UnitPrice <= maxPrice.Value);

        return query.ToListAsync();
    }

    public async Task AddAsync(MedicalSupply supply)
        => await _context.MediTrack.AddAsync(supply);

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();
}
