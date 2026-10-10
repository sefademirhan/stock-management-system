using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Models;

namespace StockManagement.Services;

/// <summary>
/// Product operations are saved to the configured EF Core database.
/// </summary>
public class InventoryService(StockDbContext db)
{
    public IReadOnlyList<Product> GetAll() =>
        db.Products.AsNoTracking().OrderBy(p => p.Id).ToList();

    public Product? GetById(int id) =>
        db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);

    public Product Add(string name, string sku, decimal unitPrice, int quantity, int lowStockThreshold)
    {
        ValidateProduct(name, sku, unitPrice, lowStockThreshold);
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

        var normalizedSku = NormalizeSku(sku);
        if (db.Products.Any(p => p.SKU == normalizedSku))
            throw new InvalidOperationException("A product with this SKU already exists.");

        var product = new Product
        {
            Name = name.Trim(),
            SKU = normalizedSku,
            UnitPrice = unitPrice,
            Quantity = quantity,
            LowStockThreshold = lowStockThreshold
        };

        db.Products.Add(product);
        db.SaveChanges();

        if (quantity > 0)
        {
            db.StockMovements.Add(CreateMovement(product, quantity, "Initial"));
            db.SaveChanges();
        }

        return product;
    }

    public bool Update(int id, string name, string sku, decimal unitPrice, int lowStockThreshold)
    {
        ValidateProduct(name, sku, unitPrice, lowStockThreshold);
        var product = db.Products.Find(id);
        if (product is null)
            return false;

        var normalizedSku = NormalizeSku(sku);
        if (db.Products.Any(p => p.Id != id && p.SKU == normalizedSku))
            throw new InvalidOperationException("Another product already uses this SKU.");

        product.Name = name.Trim();
        product.SKU = normalizedSku;
        product.UnitPrice = unitPrice;
        product.LowStockThreshold = lowStockThreshold;
        db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var product = db.Products.Find(id);
        if (product is null)
            return false;

        // History contains independent product snapshots, so it is retained.
        db.Products.Remove(product);
        db.SaveChanges();
        return true;
    }

    public bool ChangeStock(int id, int amount, bool stockIn)
    {
        var product = db.Products.Find(id);
        if (product is null || amount <= 0)
            return false;

        if (!stockIn && product.Quantity < amount)
            throw new InvalidOperationException("Not enough stock for this operation.");

        var change = stockIn ? amount : -amount;
        product.Quantity += change;
        db.StockMovements.Add(CreateMovement(product, change, stockIn ? "In" : "Out"));

        // Product quantity and movement are committed in one SaveChanges.
        db.SaveChanges();
        return true;
    }

    public IReadOnlyList<Product> GetLowStockProducts() =>
        db.Products.AsNoTracking()
            .Where(p => p.Quantity <= p.LowStockThreshold)
            .OrderBy(p => p.Name)
            .ToList();

    public IReadOnlyList<StockMovement> GetHistory() =>
        db.StockMovements.AsNoTracking()
            .OrderByDescending(m => m.Id)
            .ToList();

    private static StockMovement CreateMovement(Product product, int delta, string reason) => new()
    {
        ProductId = product.Id,
        ProductSku = product.SKU,
        ProductName = product.Name,
        QuantityChange = delta,
        Reason = reason,
        OccurredAtUtc = DateTime.UtcNow
    };

    private static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();

    private static void ValidateProduct(string name, string sku, decimal unitPrice, int lowStockThreshold)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (name.Trim().Length > 200)
            throw new ArgumentException("Product name is too long.", nameof(name));
        if (sku.Trim().Length > 64)
            throw new ArgumentException("SKU is too long.", nameof(sku));
        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Price cannot be negative.");
        if (lowStockThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(lowStockThreshold),
                "Low-stock threshold cannot be negative.");
    }
}
