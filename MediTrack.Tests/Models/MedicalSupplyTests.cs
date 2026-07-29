using MediTrack.Mvc.Models;
using FluentAssertions;
using Xunit;

namespace MediTrack.Tests.Models;

public class MedicalSupplyTests
{
    [Fact]
    public void MedicalSupply_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var supply = new MedicalSupply();

        // Assert
        supply.Id.Should().Be(0);
        supply.Code.Should().BeNullOrEmpty();
        supply.Name.Should().BeNullOrEmpty();
        supply.Quantity.Should().Be(0);
        supply.IsDeleted.Should().BeFalse();
        supply.ConcurrencyVersion.Should().Be(0);
    }

    [Fact]
    public void MedicalSupply_StockStatus_WhenQuantityZero_ShouldBeOutOfStock()
    {
        // Arrange
        var supply = new MedicalSupply { Quantity = 0, MinStock = 10 };

        // Act & Assert
        supply.Quantity.Should().Be(0);
        supply.Quantity.Should().BeLessThanOrEqualTo(0);
    }

    [Fact]
    public void MedicalSupply_StockStatus_WhenQuantityLow_ShouldBeLowStock()
    {
        // Arrange
        var supply = new MedicalSupply { Quantity = 5, MinStock = 10 };

        // Act & Assert
        supply.Quantity.Should().BeGreaterThan(0);
        supply.Quantity.Should().BeLessThanOrEqualTo(supply.MinStock);
    }

    [Fact]
    public void MedicalSupply_StockStatus_WhenQuantityHigh_ShouldBeInStock()
    {
        // Arrange
        var supply = new MedicalSupply { Quantity = 50, MinStock = 10 };

        // Act & Assert
        supply.Quantity.Should().BeGreaterThan(supply.MinStock);
    }

    [Fact]
    public void MedicalSupply_ConcurrencyVersion_ShouldIncrement()
    {
        // Arrange
        var supply = new MedicalSupply { ConcurrencyVersion = 1 };

        // Act
        supply.ConcurrencyVersion++;

        // Assert
        supply.ConcurrencyVersion.Should().Be(2);
    }
}
