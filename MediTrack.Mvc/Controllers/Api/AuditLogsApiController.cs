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
    public async Task<IActionResult> GetAll()
    {
        var logs = await _db.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new
            {
                id = l.Id.ToString(),
                action = l.Action,
                entity = l.EntityName,
                entity_id = l.EntityId,
                details = l.Note,
                performed_by = l.UserName ?? "system",
                created_at = l.CreatedAt.ToString("o")
            })
            .ToListAsync();

        return Ok(logs);
    }
}
