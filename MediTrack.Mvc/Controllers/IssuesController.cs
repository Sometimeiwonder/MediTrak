using MediTrack.Mvc.Services;
using MediTrack.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Mvc.Controllers;

[Authorize(Policy = "CanManageIssue")]
public class IssuesController : Controller
{
    private readonly IIssueService _issueService;
    private readonly IAuditLogService _auditLogService;

    public IssuesController(IIssueService issueService, IAuditLogService auditLogService)
    {
        _issueService = issueService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var issues = await _issueService.GetIssueListAsync();
        var vm = issues.Select(i => new IssueListItemViewModel
        {
            Id = i.Id,
            IssuedTo = i.IssuedTo,
            IssuedAt = i.IssuedAt,
            TotalAmount = i.TotalAmount,
            ItemCount = i.IssueItems.Count
        }).ToList();
        return View(vm);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var issue = await _issueService.GetIssueDetailAsync(id);
        if (issue == null) return NotFound();
        return View(issue);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var supplies = await _issueService.GetAvailableSuppliesAsync();
        var vm = new IssueCreateViewModel { AvailableSupplies = supplies };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IssueCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableSupplies = await _issueService.GetAvailableSuppliesAsync();
            return View(model);
        }

        try
        {
            await _issueService.CreateIssueAsync(model);
            await _auditLogService.LogAsync("Create", "Issue", null, "Success");
            TempData["SuccessMessage"] = "Tạo phiếu xuất thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AvailableSupplies = await _issueService.GetAvailableSuppliesAsync();
            return View(model);
        }
    }
}
