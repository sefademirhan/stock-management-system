using StockManagement.Services;

namespace StockManagement.Tests;

public class InventoryServiceTests
{
    [Fact]
    public void AddingProduct_AssignsIdAndSavesProduct()
    {
        var inventory = new InventoryService();

        var product = inventory.Add("Keyboard", "KEY-001", 250m, 10, 3);

        Assert.Equal(1, product.Id);
        Assert.Single(inventory.GetAll());
        Assert.Equal(10, inventory.GetById(product.Id)!.Quantity);
    }

    [Fact]
    public void DuplicateSku_IsRejectedIgnoringCaseAndWhitespace()
    {
        var inventory = new InventoryService();
        inventory.Add("Keyboard", "KEY-001", 250m, 10, 3);

        Assert.Throws<InvalidOperationException>(
            () => inventory.Add("Another", " key-001 ", 100m, 5, 2));
    }

    [Fact]
    public void StockOut_NeverAllowsNegativeStock()
    {
        var inventory = new InventoryService();
        var product = inventory.Add("Mouse", "MOU-001", 100m, 2, 1);

        Assert.Throws<InvalidOperationException>(
            () => inventory.ChangeStock(product.Id, 3, stockIn: false));

        Assert.Equal(2, product.Quantity);
    }

    [Fact]
    public void StockMovements_UpdateQuantity()
    {
        var inventory = new InventoryService();
        var product = inventory.Add("Monitor", "MON-001", 700m, 4, 2);

        Assert.True(inventory.ChangeStock(product.Id, 3, stockIn: true));
        Assert.True(inventory.ChangeStock(product.Id, 5, stockIn: false));
        Assert.Equal(2, product.Quantity);
    }

    [Fact]
    public void LowStockAlert_UsesThreshold()
    {
        var inventory = new InventoryService();
        var mouse = inventory.Add("Mouse", "MOU-001", 100m, 3, 3);
        inventory.Add("Monitor", "MON-001", 700m, 10, 3);

        var lowStock = inventory.GetLowStockProducts().ToList();

        Assert.Single(lowStock);
        Assert.Equal(mouse.Id, lowStock[0].Id);
    }

    [Fact]
    public void NegativeInitialStock_IsRejected()
    {
        var inventory = new InventoryService();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => inventory.Add("Mouse", "MOU-001", 100m, -1, 2));
    }
}
