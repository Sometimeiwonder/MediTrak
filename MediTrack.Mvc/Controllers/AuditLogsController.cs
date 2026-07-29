using MediTrack.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Mvc.Controllers;

[Authorize(Policy = "CanViewAuditLog")]
public class AuditLogsController : Controller
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(string? userName, string? logAction, string? result,
        DateTime? fromDate, DateTime? toDate)
    {
        var logs = await _auditLogService.GetAuditLogsAsync(userName, logAction, result, fromDate, toDate);

        ViewBag.UserName = userName ?? "";
        ViewBag.LogAction = logAction ?? "";
        ViewBag.Result = result ?? "";
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd") ?? "";
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd") ?? "";

        return View(logs);
    }
}
