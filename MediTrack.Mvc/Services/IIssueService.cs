using MediTrack.Mvc.Models;
using MediTrack.Mvc.ViewModels;

namespace MediTrack.Mvc.Services;

public interface IIssueService
{
    Task<List<Issue>> GetIssueListAsync();
    Task<Issue?> GetIssueDetailAsync(int id);
    Task<List<MedicalSupply>> GetAvailableSuppliesAsync();
    Task CreateIssueAsync(IssueCreateViewModel model);
}
