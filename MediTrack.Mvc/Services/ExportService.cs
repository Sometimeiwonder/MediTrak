using System.Text;
using MediTrack.Mvc.Models;
using OfficeOpenXml;

namespace MediTrack.Mvc.Services;

public class ExportService : IExportService
{
    public async Task<byte[]> ExportToExcelAsync(List<MedicalSupply> supplies)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Supplies");

        // Headers
        worksheet.Cells[1, 1].Value = "Code";
        worksheet.Cells[1, 2].Value = "Name";
        worksheet.Cells[1, 3].Value = "Category";
        worksheet.Cells[1, 4].Value = "Supplier";
        worksheet.Cells[1, 5].Value = "Unit Price";
        worksheet.Cells[1, 6].Value = "Quantity";
        worksheet.Cells[1, 7].Value = "Min Stock";
        worksheet.Cells[1, 8].Value = "Status";
        worksheet.Cells[1, 9].Value = "Created At";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 9])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(54, 162, 235));
            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        // Data
        for (int i = 0; i < supplies.Count; i++)
        {
            var supply = supplies[i];
            worksheet.Cells[i + 2, 1].Value = supply.Code;
            worksheet.Cells[i + 2, 2].Value = supply.Name;
            worksheet.Cells[i + 2, 3].Value = supply.SupplyCategory?.Name ?? "N/A";
            worksheet.Cells[i + 2, 4].Value = supply.Supplier;
            worksheet.Cells[i + 2, 5].Value = supply.UnitPrice;
            worksheet.Cells[i + 2, 6].Value = supply.Quantity;
            worksheet.Cells[i + 2, 7].Value = supply.MinStock;
            worksheet.Cells[i + 2, 8].Value = supply.Quantity <= 0 ? "Out of Stock" :
                                               supply.Quantity <= supply.MinStock ? "Low Stock" : "In Stock";
            worksheet.Cells[i + 2, 9].Value = supply.CreatedAt.ToString("dd/MM/yyyy HH:mm");
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return await package.GetAsByteArrayAsync();
    }

    public string ExportToCsv(List<MedicalSupply> supplies)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Category,Supplier,Unit Price,Quantity,Min Stock,Status,Created At");

        foreach (var supply in supplies)
        {
            var status = supply.Quantity <= 0 ? "Out of Stock" :
                        supply.Quantity <= supply.MinStock ? "Low Stock" : "In Stock";

            sb.AppendLine($"\"{supply.Code}\",\"{supply.Name}\",\"{supply.SupplyCategory?.Name ?? "N/A"}\",\"{supply.Supplier}\",{supply.UnitPrice},{supply.Quantity},{supply.MinStock},\"{status}\",\"{supply.CreatedAt:dd/MM/yyyy HH:mm}\"");
        }

        return sb.ToString();
    }
}
