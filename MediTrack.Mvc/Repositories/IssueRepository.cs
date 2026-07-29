using MediTrack.Mvc.Data;
using MediTrack.Mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly AppDbContext _context;

    public IssueRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Issue>> GetAllReadOnlyAsync()
        => _context.Issues
            .Include(i => i.IssueItems)
            .ThenInclude(ii => ii.MedicalSupply)
            .AsNoTracking()
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync();

    public Task<Issue?> GetByIdAsync(int id)
        => _context.Issues
            .Include(i => i.IssueItems)
            .ThenInclude(ii => ii.MedicalSupply)
            .FirstOrDefaultAsync(i => i.Id == id);

    public Task<MedicalSupply?> GetSupplyByIdAsync(int id)
        => _context.MediTrack.FindAsync(id).AsTask();

    public async Task AddIssueAsync(Issue issue)
        => await _context.Issues.AddAsync(issue);

    public async Task AddIssueItemAsync(IssueItem item)
        => await _context.IssueItems.AddAsync(item);

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();
}
