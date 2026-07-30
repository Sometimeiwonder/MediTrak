using MediTrack.Mvc.Data;
using MediTrack.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
public class IssuesController : ControllerBase
{
    private readonly AppDbContext _db;

    public IssuesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Issues
            .Include(i => i.IssueItems)
                .ThenInclude(ii => ii.MedicalSupply);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var issues = await query
            .OrderByDescending(i => i.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new
            {
                id = i.Id.ToString(),
                issued_to = i.IssuedTo,
                issued_at = i.IssuedAt.ToString("o"),
                total_amount = i.TotalAmount,
                item_count = i.IssueItems.Count,
                items = i.IssueItems.Select(ii => new
                {
                    supply_id = ii.MedicalSupplyId.ToString(),
                    supply_name = ii.MedicalSupply != null ? ii.MedicalSupply.Name : "",
                    quantity = ii.Quantity,
                    unit_price = ii.UnitPrice
                })
            })
            .ToListAsync();

        return Ok(new { items = issues, totalCount, page, pageSize, totalPages });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        if (!int.TryParse(id, out var intId)) return BadRequest();

        var issue = await _db.Issues
            .Include(i => i.IssueItems)
                .ThenInclude(ii => ii.MedicalSupply)
            .FirstOrDefaultAsync(i => i.Id == intId);

        if (issue == null) return NotFound();

        return Ok(new
        {
            id = issue.Id.ToString(),
            issued_to = issue.IssuedTo,
            issued_at = issue.IssuedAt.ToString("o"),
            total_amount = issue.TotalAmount,
            item_count = issue.IssueItems.Count,
            items = issue.IssueItems.Select(ii => new
            {
                id = ii.Id.ToString(),
                supply_id = ii.MedicalSupplyId.ToString(),
                supply_name = ii.MedicalSupply != null ? ii.MedicalSupply.Name : "",
                supply_code = ii.MedicalSupply != null ? ii.MedicalSupply.Code : "",
                quantity = ii.Quantity,
                unit_price = ii.UnitPrice,
                subtotal = ii.Quantity * ii.UnitPrice
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIssueRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.issued_to))
            return BadRequest(new { error = "issued_to is required" });

        if (request.items == null || request.items.Count == 0)
            return BadRequest(new { error = "At least one item is required" });

        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            decimal totalAmount = 0;
            var issueItems = new List<IssueItem>();

            foreach (var item in request.items)
            {
                if (!int.TryParse(item.supply_id, out var supplyId))
                    return BadRequest(new { error = $"Invalid supply_id: {item.supply_id}" });

                var supply = await _db.MediTrack.FindAsync(supplyId);
                if (supply == null)
                    return BadRequest(new { error = $"Supply not found: {supplyId}" });

                if (supply.Quantity < item.quantity)
                    return BadRequest(new { error = $"Insufficient stock for {supply.Name}. Available: {supply.Quantity}, Requested: {item.quantity}" });

                supply.Quantity -= item.quantity;
                totalAmount += supply.UnitPrice * item.quantity;

                issueItems.Add(new IssueItem
                {
                    MedicalSupplyId = supplyId,
                    Quantity = item.quantity,
                    UnitPrice = supply.UnitPrice
                });
            }

            var issue = new Issue
            {
                IssuedTo = request.issued_to,
                IssuedAt = DateTime.Now,
                TotalAmount = totalAmount
            };
            _db.Issues.Add(issue);
            await _db.SaveChangesAsync();

            foreach (var item in issueItems)
            {
                item.IssueId = issue.Id;
                _db.IssueItems.Add(item);
            }
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetById), new { id = issue.Id.ToString() }, new { id = issue.Id.ToString() });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public class CreateIssueRequest
{
    public string issued_to { get; set; } = "";
    public List<IssueItemRequest>? items { get; set; }
}

public class IssueItemRequest
{
    public string supply_id { get; set; } = "";
    public int quantity { get; set; }
}
