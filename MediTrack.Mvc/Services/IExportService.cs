using MediTrack.Mvc.Models;

namespace MediTrack.Mvc.Services;

public interface IExportService
{
    Task<byte[]> ExportToExcelAsync(List<MedicalSupply> supplies);
    string ExportToCsv(List<MedicalSupply> supplies);
}
