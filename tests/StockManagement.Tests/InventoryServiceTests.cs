using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Services;
using Xunit;

namespace StockManagement.Tests;

public class InventoryServiceTests
{
    private static StockDbContext CreateDatabase(SqliteConnection connection)
    {
        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();

        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new StockDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public void AddingProduct_AssignsIdAndSavesProduct()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDatabase(connection);
        var inventory = new InventoryService(db);

        var product = inventory.Add("Keyboard", "KEY-001", 250m, 10, 3);

        Assert.Equal(1, product.Id);
        Assert.Single(inventory.GetAll());
        Assert.Equal(10, inventory.GetById(product.Id)!.Quantity);
    }

    [Fact]
    public void DuplicateSku_IsRejectedIgnoringCaseAndWhitespace()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDatabase(connection);
        var inventory = new InventoryService(db);
        inventory.Add("Keyboard", "KEY-001", 250m, 10, 3);

        Assert.Throws<InvalidOperationException>(
            () => inventory.Add("Another", " key-001 ", 100m, 5, 2));
    }

    [Fact]
    public void StockOut_NeverAllowsNegativeStock()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDatabase(connection);
        var inventory = new InventoryService(db);
        var product = inventory.Add("Mouse", "MOU-001", 100m, 2, 1);

        Assert.Throws<InvalidOperationException>(
            () => inventory.ChangeStock(product.Id, 3, stockIn: false));

        Assert.Equal(2, inventory.GetById(product.Id)!.Quantity);
    }

    [Fact]
    public void StockMovements_UpdateQuantityAndKeepHistory()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDatabase(connection);
        var inventory = new InventoryService(db);
        var product = inventory.Add("Monitor", "MON-001", 700m, 4, 2);

        Assert.True(inventory.ChangeStock(product.Id, 3, stockIn: true));
        Assert.True(inventory.ChangeStock(product.Id, 5, stockIn: false));
        Assert.Equal(2, inventory.GetById(product.Id)!.Quantity);
        Assert.Equal(3, inventory.GetHistory().Count);
    }

    [Fact]
    public void LowStockAlert_UsesThreshold()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDatabase(connection);
        var inventory = new InventoryService(db);
        var mouse = inventory.Add("Mouse", "MOU-001", 100m, 3, 3);
        inventory.Add("Monitor", "MON-001", 700m, 10, 3);

        var lowStock = inventory.GetLowStockProducts();

        Assert.Single(lowStock);
        Assert.Equal(mouse.Id, lowStock[0].Id);
    }

    [Fact]
    public void NegativeInitialStock_IsRejected()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDatabase(connection);
        var inventory = new InventoryService(db);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => inventory.Add("Mouse", "MOU-001", 100m, -1, 2));
    }

    [Fact]
    public void SavedProductsRemainAfterContextIsReopened()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        int savedId;
        using (var firstDb = CreateDatabase(connection))
        {
            var service = new InventoryService(firstDb);
            savedId = service.Add("SSD", "SSD-001", 800m, 8, 2).Id;
        }

        using var secondDb = CreateDatabase(connection);
        var reopenedService = new InventoryService(secondDb);

        Assert.Equal(8, reopenedService.GetById(savedId)!.Quantity);
        Assert.Single(reopenedService.GetHistory());
    }

    [Fact]
    public void DeletingProductPreservesMovementHistory()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        using var db = CreateDatabase(connection);
        var inventory = new InventoryService(db);
        var product = inventory.Add("Keyboard", "KEY-001", 250m, 3, 1);

        Assert.True(inventory.Delete(product.Id));
        Assert.Empty(inventory.GetAll());
        Assert.Single(inventory.GetHistory());
    }
}
