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
    public async Task<IActionResult> GetAll()
    {
        var issues = await _db.Issues
            .Include(i => i.IssueItems)
                .ThenInclude(ii => ii.MedicalSupply)
            .OrderByDescending(i => i.IssuedAt)
            .Select(i => new
            {
                id = i.Id.ToString(),
                supply_id = i.IssueItems.FirstOrDefault() != null ? i.IssueItems.First().MedicalSupplyId.ToString() : "",
                quantity = i.IssueItems.FirstOrDefault() != null ? i.IssueItems.First().Quantity : 0,
                issued_to = i.IssuedTo,
                issued_by = "system",
                notes = (string?)null,
                created_at = i.IssuedAt.ToString("o"),
                supply = i.IssueItems.FirstOrDefault() != null && i.IssueItems.First().MedicalSupply != null ? new
                {
                    id = i.IssueItems.First().MedicalSupply.Id.ToString(),
                    name = i.IssueItems.First().MedicalSupply.Name,
                    sku = i.IssueItems.First().MedicalSupply.Code,
                    quantity = i.IssueItems.First().MedicalSupply.Quantity,
                    unit = "units"
                } : null
            })
            .ToListAsync();

        return Ok(issues);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIssueRequest request)
    {
        if (!int.TryParse(request.supply_id, out var supplyId))
            return BadRequest("Invalid supply_id");

        var supply = await _db.MediTrack.FindAsync(supplyId);
        if (supply == null) return NotFound("Supply not found");

        if (supply.Quantity < request.quantity)
            return BadRequest("Insufficient stock");

        var issue = new Issue
        {
            IssuedTo = request.issued_to,
            IssuedAt = DateTime.Now,
            TotalAmount = supply.UnitPrice * request.quantity
        };
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();

        var item = new IssueItem
        {
            IssueId = issue.Id,
            MedicalSupplyId = supplyId,
            Quantity = request.quantity,
            UnitPrice = supply.UnitPrice
        };
        _db.IssueItems.Add(item);

        supply.Quantity -= request.quantity;
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = issue.Id.ToString() }, new { id = issue.Id.ToString() });
    }
}

public class CreateIssueRequest
{
    public string supply_id { get; set; } = "";
    public int quantity { get; set; }
    public string issued_to { get; set; } = "";
}
