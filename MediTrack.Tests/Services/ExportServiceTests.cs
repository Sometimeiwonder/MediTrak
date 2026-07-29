using MediTrack.Mvc.Models;
using MediTrack.Mvc.Services;
using FluentAssertions;
using Xunit;

namespace MediTrack.Tests.Services;

public class ExportServiceTests
{
    private readonly IExportService _exportService;

    public ExportServiceTests()
    {
        _exportService = new ExportService();
    }

    [Fact]
    public void ExportToCsv_ShouldReturnCorrectContent()
    {
        // Arrange
        var supplies = new List<MedicalSupply>
        {
            new() { Code = "MED001", Name = "Surgical Mask", Supplier = "MedCorp", UnitPrice = 5000, Quantity = 100, MinStock = 20, CreatedAt = DateTime.Now },
            new() { Code = "MED002", Name = "Hand Sanitizer", Supplier = "CleanCo", UnitPrice = 25000, Quantity = 50, MinStock = 10, CreatedAt = DateTime.Now }
        };

        // Act
        var result = _exportService.ExportToCsv(supplies);

        // Assert
        result.Should().Contain("Code,Name,Category,Supplier");
        result.Should().Contain("MED001");
        result.Should().Contain("Surgical Mask");
        result.Should().Contain("MED002");
        result.Should().Contain("Hand Sanitizer");
    }

    [Fact]
    public void ExportToCsv_EmptyList_ShouldReturnOnlyHeaders()
    {
        // Arrange
        var supplies = new List<MedicalSupply>();

        // Act
        var result = _exportService.ExportToCsv(supplies);

        // Assert
        result.Should().Contain("Code,Name,Category,Supplier");
        result.Should().NotContainAny("MED", "Mask");
    }

    [Fact]
    public async Task ExportToExcel_ShouldReturnNonEmptyByteArray()
    {
        // Arrange
        var supplies = new List<MedicalSupply>
        {
            new() { Code = "MED001", Name = "Test Supply", Supplier = "TestCo", UnitPrice = 10000, Quantity = 25, MinStock = 5, CreatedAt = DateTime.Now }
        };

        // Act
        var result = await _exportService.ExportToExcelAsync(supplies);

        // Assert
        result.Should().NotBeEmpty();
        result.Length.Should().BeGreaterThan(0);
    }
}
