using MediTrack.Mvc.Data;
using MediTrack.Mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityName, string? entityId, string result, string? note = null)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name,
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            Result = result,
            Note = note,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetAuditLogsAsync(string? userName, string? action, string? result, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(a => a.UserName != null && a.UserName.Contains(userName));

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action.Contains(action));

        if (!string.IsNullOrWhiteSpace(result))
            query = query.Where(a => a.Result == result);

        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= fromDate.Value.ToUniversalTime());

        if (toDate.HasValue)
            query = query.Where(a => a.CreatedAt <= toDate.Value.ToUniversalTime().AddDays(1));

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<int> GetAccessDeniedCountTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.AuditLogs
            .CountAsync(a => a.Action == "AccessDenied" && a.CreatedAt >= today);
    }

    public async Task<int> GetSensitiveActionsCountTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var sensitiveActions = new[] { "Create", "Edit", "Delete", "Restore", "ReplaceProductImage", "AdjustStock" };
        return await _context.AuditLogs
            .CountAsync(a => sensitiveActions.Contains(a.Action) && a.CreatedAt >= today);
    }

    public async Task<int> GetRejectedUploadsCountTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.AuditLogs
            .CountAsync(a => a.Action == "ReplaceProductImage" && a.Result == "Rejected" && a.CreatedAt >= today);
    }
}
