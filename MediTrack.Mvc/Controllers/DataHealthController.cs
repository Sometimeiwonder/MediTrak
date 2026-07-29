using MediTrack.Mvc.Data;
using MediTrack.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Controllers;

[Authorize]
public class DataHealthController : Controller
{
    private readonly AppDbContext _context;

    public DataHealthController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new DataHealthViewModel();

        vm.DatabaseExists = _context.Database.CanConnect();

        var appliedMigrations = _context.Database.GetAppliedMigrations().ToList();
        var pendingMigrations = _context.Database.GetPendingMigrations().ToList();
        var allMigrations = appliedMigrations.Concat(pendingMigrations).ToList();
        vm.MigrationCount = appliedMigrations.Count;
        vm.LastMigrationName = appliedMigrations.LastOrDefault() ?? "None";

        vm.TotalCategories = await _context.SupplyCategories.CountAsync();
        vm.TotalSupplies = await _context.MediTrack.CountAsync();
        vm.TotalIssues = await _context.Issues.CountAsync();
        vm.TotalIssueItems = await _context.IssueItems.CountAsync();

        var hasCategories = await _context.SupplyCategories.AnyAsync();
        var hasSupplies = await _context.MediTrack.AnyAsync();
        vm.SeedStatus.Add(hasCategories ? "SupplyCategories: OK" : "SupplyCategories: EMPTY");
        vm.SeedStatus.Add(hasSupplies ? "MediTrack: OK" : "MediTrack: EMPTY");

        var supplyWithTracking = await _context.MediTrack.FirstAsync();
        var supplyNoTracking = await _context.MediTrack.AsNoTracking().FirstAsync();
        vm.TrackingDemoResult = $"Tracking - Entry State: {_context.Entry(supplyWithTracking).State}; " +
                                $"NoTracking - Entity isDetached: {_context.Entry(supplyNoTracking).State == EntityState.Detached}";

        return View(vm);
    }
}
