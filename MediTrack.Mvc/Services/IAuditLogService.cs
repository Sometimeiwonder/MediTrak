using MediTrack.Mvc.Models;

namespace MediTrack.Mvc.Services;

public interface IAuditLogService
{
    Task LogAsync(string action, string entityName, string? entityId, string result, string? note = null);
    Task<List<AuditLog>> GetAuditLogsAsync(string? userName, string? action, string? result, DateTime? fromDate, DateTime? toDate);
    Task<int> GetAccessDeniedCountTodayAsync();
    Task<int> GetSensitiveActionsCountTodayAsync();
    Task<int> GetRejectedUploadsCountTodayAsync();
}
