using MediTrack.Mvc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditLogsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? userName = null,
        [FromQuery] string? action = null,
        [FromQuery] string? result = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(l => l.UserName != null && l.UserName.Contains(userName));

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action == action);

        if (!string.IsNullOrWhiteSpace(result))
            query = query.Where(l => l.Result == result);

        if (fromDate.HasValue)
            query = query.Where(l => l.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.CreatedAt <= toDate.Value.AddDays(1));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                id = l.Id.ToString(),
                action = l.Action,
                entity = l.EntityName,
                entity_id = l.EntityId,
                details = l.Note,
                performed_by = l.UserName ?? "system",
                result = l.Result,
                ip_address = l.IpAddress,
                created_at = l.CreatedAt.ToString("o")
            })
            .ToListAsync();

        return Ok(new { items = logs, totalCount, page, pageSize, totalPages });
    }
}
