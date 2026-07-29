using MediTrack.Mvc.Data;
using MediTrack.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Controllers;

[Authorize(Policy = "CanViewSupply")]
public class SupplyCategoriesController : Controller
{
    private readonly AppDbContext _context;

    public SupplyCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.SupplyCategories
            .Include(c => c.Supplies)
            .AsNoTracking()
            .Select(c => new SupplyCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                SupplyCount = c.Supplies.Count,
                TotalInventoryValue = c.Supplies.Sum(s => s.UnitPrice * s.Quantity)
            })
            .ToListAsync();

        return View(categories);
    }
}
