using MediTrack.Mvc.Data;
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
                description = (string?)null,
                created_at = DateTime.UtcNow.ToString("o")
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var category = new Models.SupplyCategory { Name = request.name };
        _db.SupplyCategories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = category.Id.ToString() }, new { id = category.Id.ToString() });
    }
}

public class CreateCategoryRequest
{
    public string name { get; set; } = "";
}
