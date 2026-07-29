using MediTrack.Mvc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
public class SuppliesController : ControllerBase
{
    private readonly AppDbContext _db;

    public SuppliesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var supplies = await _db.MediTrack
            .Include(s => s.SupplyCategory)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                id = s.Id.ToString(),
                name = s.Name,
                sku = s.Code,
                category_id = s.SupplyCategoryId.ToString(),
                quantity = s.Quantity,
                unit = "units",
                reorder_level = s.MinStock,
                expiry_date = (string?)null,
                location = (string?)null,
                created_at = s.CreatedAt.ToString("o"),
                category = s.SupplyCategory != null ? new
                {
                    id = s.SupplyCategory.Id.ToString(),
                    name = s.SupplyCategory.Name,
                    description = (string?)null,
                    created_at = DateTime.UtcNow.ToString("o")
                } : null
            })
            .ToListAsync();

        return Ok(supplies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!int.TryParse(id, out var intId)) return BadRequest();

        var supply = await _db.MediTrack
            .Include(s => s.SupplyCategory)
            .FirstOrDefaultAsync(s => s.Id == intId && !s.IsDeleted);

        if (supply == null) return NotFound();

        return Ok(new
        {
            id = supply.Id.ToString(),
            name = supply.Name,
            sku = supply.Code,
            category_id = supply.SupplyCategoryId.ToString(),
            quantity = supply.Quantity,
            unit = "units",
            reorder_level = supply.MinStock,
            expiry_date = (string?)null,
            location = (string?)null,
            created_at = supply.CreatedAt.ToString("o"),
            category = supply.SupplyCategory != null ? new
            {
                id = supply.SupplyCategory.Id.ToString(),
                name = supply.SupplyCategory.Name,
                description = (string?)null,
                created_at = DateTime.UtcNow.ToString("o")
            } : null
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplyRequest request)
    {
        var supply = new Models.MedicalSupply
        {
            Code = request.sku,
            Name = request.name,
            SupplyCategoryId = int.TryParse(request.category_id, out var catId) ? catId : 0,
            Quantity = request.quantity,
            MinStock = request.reorder_level,
            UnitPrice = 0,
            Supplier = "",
            CreatedAt = DateTime.Now,
            IsDeleted = false,
            ConcurrencyVersion = 1
        };

        _db.MediTrack.Add(supply);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = supply.Id.ToString() }, new { id = supply.Id.ToString() });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateSupplyRequest request)
    {
        if (!int.TryParse(id, out var intId)) return BadRequest();

        var supply = await _db.MediTrack.FindAsync(intId);
        if (supply == null) return NotFound();

        supply.Name = request.name;
        supply.Code = request.sku;
        if (int.TryParse(request.category_id, out var catId))
            supply.SupplyCategoryId = catId;
        supply.Quantity = request.quantity;
        supply.MinStock = request.reorder_level;
        supply.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return Ok(new { id = supply.Id.ToString() });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!int.TryParse(id, out var intId)) return BadRequest();

        var supply = await _db.MediTrack.FindAsync(intId);
        if (supply == null) return NotFound();

        supply.IsDeleted = true;
        supply.DeletedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateSupplyRequest
{
    public string name { get; set; } = "";
    public string sku { get; set; } = "";
    public string? category_id { get; set; }
    public int quantity { get; set; }
    public int reorder_level { get; set; }
}

public class UpdateSupplyRequest
{
    public string name { get; set; } = "";
    public string sku { get; set; } = "";
    public string? category_id { get; set; }
    public int quantity { get; set; }
    public int reorder_level { get; set; }
}
