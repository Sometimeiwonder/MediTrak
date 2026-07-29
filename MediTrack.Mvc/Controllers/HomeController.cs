using MediTrack.Mvc.Models;
using MediTrack.Mvc.Services;
using MediTrack.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Mvc.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ISupplyService _supplyService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        ISupplyService supplyService,
        IAuditLogService auditLogService,
        ILogger<HomeController> logger)
    {
        _supplyService = supplyService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = await _supplyService.GetDashboardAsync();
        dashboard.AccessDeniedToday = await _auditLogService.GetAccessDeniedCountTodayAsync();
        dashboard.SensitiveActionsToday = await _auditLogService.GetSensitiveActionsCountTodayAsync();
        dashboard.RejectedUploadsToday = await _auditLogService.GetRejectedUploadsCountTodayAsync();
        return View(dashboard);
    }

    public IActionResult StatusCode(int? code)
    {
        ViewData["StatusCode"] = code;
        return View();
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
