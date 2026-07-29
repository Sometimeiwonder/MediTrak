using MediTrack.Mvc.Models;

namespace MediTrack.Mvc.Repositories;

public interface IIssueRepository
{
    Task<List<Issue>> GetAllReadOnlyAsync();
    Task<Issue?> GetByIdAsync(int id);
    Task<MedicalSupply?> GetSupplyByIdAsync(int id);
    Task AddIssueAsync(Issue issue);
    Task AddIssueItemAsync(IssueItem item);
    Task SaveChangesAsync();
}
