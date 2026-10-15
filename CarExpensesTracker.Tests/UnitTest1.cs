using Xunit;
using CarExpensesTracker.Models;

public class CalculationTests
{
    [Fact]
    public void TestTotalCostCalculation()
    {
        var record = new FuelRecord { Liters = 10, PricePerLiter = 50.0m };
        Assert.Equal(500.0m, record.TotalCost);
    }

    [Fact]
    public void TestAnotherCalculation()
    {
        var record = new FuelRecord { Liters = 20, PricePerLiter = 45.0m };
        Assert.Equal(900.0m, record.TotalCost);
    }
}