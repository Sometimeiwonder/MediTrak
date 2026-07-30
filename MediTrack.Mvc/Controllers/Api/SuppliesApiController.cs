using MediTrack.Mvc.Data;
using MediTrack.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
public class SuppliesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public SuppliesController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? stockStatus = null,
        [FromQuery] int? categoryId = null)
    {
        var query = _db.MediTrack
            .Include(s => s.SupplyCategory)
            .Where(s => !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(kw) || s.Code.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(stockStatus))
        {
            query = stockStatus.ToLower() switch
            {
                "outofstock" => query.Where(s => s.Quantity <= 0),
                "lowstock" => query.Where(s => s.Quantity > 0 && s.Quantity <= s.MinStock),
                "instock" => query.Where(s => s.Quantity > s.MinStock),
                _ => query
            };
        }

        if (categoryId.HasValue)
            query = query.Where(s => s.SupplyCategoryId == categoryId.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                id = s.Id.ToString(),
                name = s.Name,
                sku = s.Code,
                category_id = s.SupplyCategoryId.ToString(),
                quantity = s.Quantity,
                unit = "units",
                reorder_level = s.MinStock,
                supplier = s.Supplier,
                unit_price = s.UnitPrice,
                description = s.Description,
                image_url = s.ImageUrl,
                created_at = s.CreatedAt.ToString("o"),
                updated_at = s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("o") : (string?)null,
                category = s.SupplyCategory != null ? new
                {
                    id = s.SupplyCategory.Id.ToString(),
                    name = s.SupplyCategory.Name
                } : null
            })
            .ToListAsync();

        return Ok(new { items, totalCount, page, pageSize, totalPages });
    }

    [HttpGet("trash")]
    public async Task<IActionResult> GetTrash()
    {
        var items = await _db.MediTrack
            .IgnoreQueryFilters()
            .Include(s => s.SupplyCategory)
            .Where(s => s.IsDeleted)
            .OrderByDescending(s => s.DeletedAt)
            .Select(s => new
            {
                id = s.Id.ToString(),
                name = s.Name,
                sku = s.Code,
                category_name = s.SupplyCategory != null ? s.SupplyCategory.Name : "",
                quantity = s.Quantity,
                deleted_at = s.DeletedAt.HasValue ? s.DeletedAt.Value.ToString("o") : (string?)null,
                created_at = s.CreatedAt.ToString("o")
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var supplies = await _db.MediTrack.Where(s => !s.IsDeleted).ToListAsync();
        return Ok(new
        {
            totalSupplies = supplies.Count,
            totalQuantity = supplies.Sum(s => s.Quantity),
            totalValue = supplies.Sum(s => s.Quantity * (double)s.UnitPrice),
            outOfStock = supplies.Count(s => s.Quantity <= 0),
            needReorder = supplies.Count(s => s.Quantity <= s.MinStock)
        });
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
            supplier = supply.Supplier,
            unit_price = supply.UnitPrice,
            description = supply.Description,
            image_url = supply.ImageUrl,
            concurrency_version = supply.ConcurrencyVersion,
            created_at = supply.CreatedAt.ToString("o"),
            updated_at = supply.UpdatedAt?.ToString("o"),
            category = supply.SupplyCategory != null ? new
            {
                id = supply.SupplyCategory.Id.ToString(),
                name = supply.SupplyCategory.Name
            } : null
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplyRequest request)
    {
        var supply = new MedicalSupply
        {
            Code = request.sku,
            Name = request.name,
            SupplyCategoryId = int.TryParse(request.category_id, out var catId) ? catId : 0,
            Quantity = request.quantity,
            MinStock = request.reorder_level,
            Supplier = request.supplier ?? "",
            UnitPrice = request.unit_price ?? 0m,
            Description = request.description,
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
        supply.Supplier = request.supplier ?? supply.Supplier;
        supply.UnitPrice = request.unit_price ?? supply.UnitPrice;
        supply.Description = request.description ?? supply.Description;
        supply.UpdatedAt = DateTime.Now;
        supply.ConcurrencyVersion++;

        await _db.SaveChangesAsync();
        return Ok(new { id = supply.Id.ToString(), concurrency_version = supply.ConcurrencyVersion });
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

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> Restore(string id)
    {
        if (!int.TryParse(id, out var intId)) return BadRequest();

        var supply = await _db.MediTrack.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == intId && s.IsDeleted);
        if (supply == null) return NotFound();

        supply.IsDeleted = false;
        supply.DeletedAt = null;
        await _db.SaveChangesAsync();

        return Ok(new { id = supply.Id.ToString() });
    }

    [HttpPost("{id}/adjust")]
    public async Task<IActionResult> AdjustStock(string id, [FromBody] AdjustStockRequest request)
    {
        if (!int.TryParse(id, out var intId)) return BadRequest();

        var supply = await _db.MediTrack.FindAsync(intId);
        if (supply == null) return NotFound();

        var newQty = supply.Quantity + request.adjustment;
        if (newQty < 0)
            return BadRequest(new { error = "Adjustment would result in negative stock" });

        supply.Quantity = newQty;
        supply.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return Ok(new { id = supply.Id.ToString(), new_quantity = supply.Quantity });
    }

    [HttpPost("{id}/upload-image")]
    public async Task<IActionResult> UploadImage(string id, IFormFile file)
    {
        if (!int.TryParse(id, out var intId)) return BadRequest();

        var supply = await _db.MediTrack.FindAsync(intId);
        if (supply == null) return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!allowed.Contains(ext))
            return BadRequest(new { error = "Invalid file type. Allowed: jpg, jpeg, png, webp" });

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { error = "File size must be under 2MB" });

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"supply_{intId}_{DateTime.Now.Ticks}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        supply.ImageUrl = $"/uploads/{fileName}";
        await _db.SaveChangesAsync();

        return Ok(new { image_url = supply.ImageUrl });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format = "csv")
    {
        var supplies = await _db.MediTrack
            .Include(s => s.SupplyCategory)
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync();

        if (format.ToLower() == "csv")
        {
            var csv = "Code,Name,Category,Supplier,UnitPrice,Quantity,MinStock,Description\n";
            foreach (var s in supplies)
            {
                csv += $"{s.Code},{s.Name},{s.SupplyCategory?.Name ?? ""},{s.Supplier},{s.UnitPrice},{s.Quantity},{s.MinStock},\"{(s.Description ?? "").Replace("\"", "\"\"")}\"\n";
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "supplies_export.csv");
        }

        return Ok(supplies.Select(s => new { s.Code, s.Name, s.Supplier, s.UnitPrice, s.Quantity, s.MinStock }));
    }
}

public class CreateSupplyRequest
{
    public string name { get; set; } = "";
    public string sku { get; set; } = "";
    public string? category_id { get; set; }
    public int quantity { get; set; }
    public int reorder_level { get; set; }
    public string? supplier { get; set; }
    public decimal? unit_price { get; set; }
    public string? description { get; set; }
}

public class UpdateSupplyRequest
{
    public string name { get; set; } = "";
    public string sku { get; set; } = "";
    public string? category_id { get; set; }
    public int quantity { get; set; }
    public int reorder_level { get; set; }
    public string? supplier { get; set; }
    public decimal? unit_price { get; set; }
    public string? description { get; set; }
}

public class AdjustStockRequest
{
    public int adjustment { get; set; }
    public string? note { get; set; }
}
