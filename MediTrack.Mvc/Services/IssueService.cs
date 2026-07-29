using MediTrack.Mvc.Data;
using MediTrack.Mvc.Models;
using MediTrack.Mvc.Repositories;
using MediTrack.Mvc.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Services;

public class IssueService : IIssueService
{
    private readonly IIssueRepository _repository;
    private readonly AppDbContext _context;

    public IssueService(IIssueRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<List<Issue>> GetIssueListAsync()
        => await _repository.GetAllReadOnlyAsync();

    public async Task<Issue?> GetIssueDetailAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<List<MedicalSupply>> GetAvailableSuppliesAsync()
        => await _context.MediTrack
            .Where(s => s.Quantity > 0)
            .AsNoTracking()
            .ToListAsync();

    public async Task CreateIssueAsync(IssueCreateViewModel model)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var supply = await _context.MediTrack.FirstOrDefaultAsync(s => s.Id == model.SupplyId);
            if (supply == null) throw new Exception("Vat tu khong ton tai.");
            if (supply.Quantity < model.Quantity) throw new Exception("So luong ton kho khong du.");

            var issue = new Issue
            {
                IssuedTo = model.IssuedTo,
                IssuedAt = DateTime.Now,
                TotalAmount = supply.UnitPrice * model.Quantity
            };
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            var item = new IssueItem
            {
                IssueId = issue.Id,
                MedicalSupplyId = supply.Id,
                Quantity = model.Quantity,
                UnitPrice = supply.UnitPrice
            };
            _context.IssueItems.Add(item);
            supply.Quantity -= model.Quantity;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
