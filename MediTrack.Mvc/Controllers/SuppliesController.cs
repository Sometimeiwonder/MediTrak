using MediTrack.Mvc.Data;
using MediTrack.Mvc.Services;
using MediTrack.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Mvc.Controllers;

[Authorize(Policy = "CanViewSupply")]
public class SuppliesController : Controller
{
    private readonly ISupplyService _supplyService;
    private readonly AppDbContext _context;
    private readonly ILogger<SuppliesController> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IExportService _exportService;

    public SuppliesController(
        ISupplyService supplyService,
        AppDbContext context,
        ILogger<SuppliesController> logger,
        IAuditLogService auditLogService,
        IFileUploadService fileUploadService,
        IExportService exportService)
    {
        _supplyService = supplyService;
        _context = context;
        _logger = logger;
        _auditLogService = auditLogService;
        _fileUploadService = fileUploadService;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var allSupplies = await _supplyService.GetActiveSuppliesAsync();
        var totalCount = allSupplies.Count;
        var pagedSupplies = allSupplies.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.TotalCount = totalCount;

        return View(pagedSupplies);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var vm = await _supplyService.GetDetailAsync(id);
        if (vm == null)
        {
            _logger.LogWarning("Khong tim thay vat tu. SupplyId={SupplyId}", id);
            return NotFound();
        }
        return View(vm);
    }

    public async Task<IActionResult> Stats()
    {
        var stats = await _supplyService.GetStatsAsync();
        return View(stats);
    }

    public async Task<IActionResult> Search(string? keyword, string? stockStatus)
    {
        var supplies = await _supplyService.SearchWithStockStatusAsync(keyword, stockStatus);
        var vm = new SupplySearchViewModel
        {
            Keyword = keyword ?? "",
            StockStatus = stockStatus ?? "",
            Supplies = supplies
        };
        return View(vm);
    }

    public async Task<IActionResult> Filter(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var categories = await _supplyService.GetCategoriesAsync();
        var results = await _supplyService.FilterSuppliesAsync(categoryId, minPrice, maxPrice);
        var vm = new SupplyFilterViewModel
        {
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Categories = categories,
            Results = results
        };
        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = "CanManageSupply")]
    public async Task<IActionResult> Create()
    {
        var categories = await _supplyService.GetCategoriesAsync();
        var vm = new SupplyCreateViewModel { Categories = categories };
        return View(vm);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageSupply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplyCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _supplyService.GetCategoriesAsync();
            return View(model);
        }

        var exists = await _context.MediTrack
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Code == model.Code);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.Code), "Ma vat tu nay da ton tai.");
            model.Categories = await _supplyService.GetCategoriesAsync();
            return View(model);
        }

        var supply = await _supplyService.CreateSupplyAsync(model);
        _logger.LogInformation("Tao vat tu thanh cong. SupplyId={SupplyId}, Code={Code}", supply.Id, supply.Code);
        await _auditLogService.LogAsync("Create", "MedicalSupply", supply.Id.ToString(), "Success");
        TempData["SuccessMessage"] = "Da them vat tu thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = "CanManageSupply")]
    public async Task<IActionResult> Edit(int id)
    {
        var supply = await _context.MediTrack
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supply == null)
        {
            _logger.LogWarning("Khong tim thay vat tu de sua. SupplyId={SupplyId}", id);
            return NotFound();
        }

        var categories = await _supplyService.GetCategoriesAsync();
        var vm = new SupplyEditViewModel
        {
            Id = supply.Id,
            Code = supply.Code,
            Name = supply.Name,
            SupplyCategoryId = supply.SupplyCategoryId,
            Supplier = supply.Supplier,
            Price = supply.UnitPrice,
            Quantity = supply.Quantity,
            MinStock = supply.MinStock,
            ImageUrl = supply.ImageUrl,
            ConcurrencyVersion = supply.ConcurrencyVersion,
            Categories = categories
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageSupply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplyEditViewModel model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            model.Categories = await _supplyService.GetCategoriesAsync();
            return View(model);
        }

        var exists = await _context.MediTrack
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Code == model.Code && s.Id != id);

        if (exists)
        {
            ModelState.AddModelError(nameof(model.Code), "Ma vat tu nay da ton tai.");
            model.Categories = await _supplyService.GetCategoriesAsync();
            return View(model);
        }

        var result = await _supplyService.UpdateSupplyAsync(model);
        if (!result)
        {
            ModelState.AddModelError(string.Empty,
                "Du lieu da duoc nguoi khac cap nhat. Vui long tai lai trang va thu lai.");
            model.Categories = await _supplyService.GetCategoriesAsync();
            return View(model);
        }

        await _auditLogService.LogAsync("Edit", "MedicalSupply", id.ToString(), "Success");
        TempData["SuccessMessage"] = "Da cap nhat vat tu thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = "CanManageSupply")]
    public async Task<IActionResult> Delete(int id)
    {
        var supply = await _context.MediTrack
            .Include(s => s.SupplyCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supply == null)
        {
            _logger.LogWarning("Khong tim thay vat tu de xoa. SupplyId={SupplyId}", id);
            return NotFound();
        }

        var vm = new SupplyDeleteViewModel
        {
            Id = supply.Id,
            Code = supply.Code,
            Name = supply.Name,
            Category = supply.SupplyCategory?.Name ?? "N/A",
            Supplier = supply.Supplier,
            UnitPrice = supply.UnitPrice,
            Quantity = supply.Quantity
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageSupply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, SupplyDeleteViewModel model)
    {
        if (id != model.Id) return NotFound();

        var result = await _supplyService.SoftDeleteSupplyAsync(id);
        if (!result)
        {
            return NotFound();
        }

        await _auditLogService.LogAsync("Delete", "MedicalSupply", id.ToString(), "Success");
        TempData["SuccessMessage"] = "Da xoa mem vat tu thanh cong.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Trash()
    {
        var deletedSupplies = await _supplyService.GetTrashAsync();
        return View(deletedSupplies);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageSupply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var result = await _supplyService.RestoreSupplyAsync(id);
        if (!result)
        {
            return NotFound();
        }

        await _auditLogService.LogAsync("Restore", "MedicalSupply", id.ToString(), "Success");
        TempData["SuccessMessage"] = "Da khoi phuc vat tu thanh cong.";
        return RedirectToAction(nameof(Trash));
    }

    [HttpGet]
    [Authorize(Policy = "CanAdjustStock")]
    public async Task<IActionResult> AdjustStock(int id)
    {
        var vm = await _supplyService.GetAdjustStockAsync(id);
        if (vm == null)
        {
            _logger.LogWarning("Khong tim thay vat tu de dieu chinh ton kho. SupplyId={SupplyId}", id);
            return NotFound();
        }

        return View(vm);
    }

    [HttpPost]
    [Authorize(Policy = "CanAdjustStock")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(int id, SupplyAdjustStockViewModel model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        if (model.NewQuantity < 0)
        {
            ModelState.AddModelError(nameof(model.Adjustment),
                "So luong sau dieu chinh khong duoc am.");
            return View(model);
        }

        var result = await _supplyService.AdjustStockAsync(model);
        if (!result)
        {
            await _auditLogService.LogAsync("AdjustStock", "MedicalSupply", id.ToString(), "Failed",
                $"Concurrency conflict. Adjustment={model.Adjustment}");
            ModelState.AddModelError(string.Empty,
                "Du lieu da duoc nguoi khac thay doi. Vui long tai lai trang va thu lai.");
            return View(model);
        }

        await _auditLogService.LogAsync("AdjustStock", "MedicalSupply", id.ToString(), "Success",
            $"Adjustment={model.Adjustment}, NewQuantity={model.NewQuantity}");
        TempData["SuccessMessage"] = "Da dieu chinh so luong thanh cong.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [Authorize(Policy = "CanManageSupply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(int id, IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Vui long chon anh.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var supply = await _context.MediTrack.FirstOrDefaultAsync(s => s.Id == id);
        if (supply == null) return NotFound();

        try
        {
            var oldImageUrl = supply.ImageUrl;
            var newImageUrl = await _fileUploadService.SaveProductImageAsync(imageFile);

            supply.ImageUrl = newImageUrl;
            supply.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                await _fileUploadService.DeleteFileAsync(oldImageUrl);
            }

            await _auditLogService.LogAsync("ReplaceProductImage", "MedicalSupply", id.ToString(), "Success",
                $"FileName={imageFile.FileName}");
            TempData["SuccessMessage"] = "Da tai anh thanh cong.";
        }
        catch (InvalidOperationException ex)
        {
            await _auditLogService.LogAsync("ReplaceProductImage", "MedicalSupply", id.ToString(), "Rejected",
                $"FileName={imageFile.FileName}, Reason={ex.Message}");
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [Authorize(Policy = "CanManageSupply")]
    public async Task<IActionResult> ExportExcel()
    {
        var supplies = await _supplyService.GetActiveSuppliesAsync();
        var medicalSupplies = supplies.Select(s => new MediTrack.Mvc.Models.MedicalSupply
        {
            Code = s.Code,
            Name = s.Name,
            Supplier = s.Supplier,
            UnitPrice = s.UnitPrice,
            Quantity = s.Quantity,
            MinStock = s.MinStock,
            CreatedAt = DateTime.Now
        }).ToList();

        var excelBytes = await _exportService.ExportToExcelAsync(medicalSupplies);
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MediTrack_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    [Authorize(Policy = "CanManageSupply")]
    public async Task<IActionResult> ExportCsv()
    {
        var supplies = await _supplyService.GetActiveSuppliesAsync();
        var medicalSupplies = supplies.Select(s => new MediTrack.Mvc.Models.MedicalSupply
        {
            Code = s.Code,
            Name = s.Name,
            Supplier = s.Supplier,
            UnitPrice = s.UnitPrice,
            Quantity = s.Quantity,
            MinStock = s.MinStock,
            CreatedAt = DateTime.Now
        }).ToList();

        var csvContent = _exportService.ExportToCsv(medicalSupplies);
        var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        return File(csvBytes, "text/csv", $"MediTrack_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }
}
