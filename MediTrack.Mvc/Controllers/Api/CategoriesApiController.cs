using MediTrack.Mvc.Data;
using MediTrack.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.SupplyCategories
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                id = c.Id.ToString(),
                name = c.Name,
                supply_count = c.Supplies.Count(s => !s.IsDeleted),
                total_inventory_value = c.Supplies
                    .Where(s => !s.IsDeleted)
                    .Sum(s => s.Quantity * (double)s.UnitPrice),
                created_at = DateTime.UtcNow.ToString("o")
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.name))
            return BadRequest(new { error = "Name is required" });

        var category = new SupplyCategory { Name = request.name };
        _db.SupplyCategories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = category.Id.ToString() }, new { id = category.Id.ToString() });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!int.TryParse(id, out var intId)) return BadRequest();

        var category = await _db.SupplyCategories
            .Include(c => c.Supplies)
            .FirstOrDefaultAsync(c => c.Id == intId);

        if (category == null) return NotFound();

        if (category.Supplies.Any(s => !s.IsDeleted))
            return BadRequest(new { error = "Cannot delete category with active supplies" });

        _db.SupplyCategories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateCategoryRequest
{
    public string name { get; set; } = "";
}
