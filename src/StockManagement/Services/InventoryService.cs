using StockManagement.Models;

namespace StockManagement.Services;

public class InventoryService
{
    private readonly List<Product> _products = [];
    private int _nextId = 1;

    public IReadOnlyList<Product> GetAll() => _products.AsReadOnly();

    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

    public Product Add(string name, string sku, decimal unitPrice, int quantity, int lowStockThreshold)
    {
        ValidateProduct(name, sku, unitPrice, lowStockThreshold);

        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

        var normalizedSku = sku.Trim();
        if (_products.Any(p => p.SKU.Equals(normalizedSku, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A product with this SKU already exists.");

        var product = new Product
        {
            Id = _nextId++,
            Name = name.Trim(),
            SKU = normalizedSku,
            UnitPrice = unitPrice,
            Quantity = quantity,
            LowStockThreshold = lowStockThreshold
        };

        _products.Add(product);
        return product;
    }

    public bool Update(int id, string name, string sku, decimal unitPrice, int lowStockThreshold)
    {
        ValidateProduct(name, sku, unitPrice, lowStockThreshold);

        var product = GetById(id);
        if (product is null)
            return false;

        var normalizedSku = sku.Trim();
        if (_products.Any(p => p.Id != id && p.SKU.Equals(normalizedSku, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Another product already uses this SKU.");

        product.Name = name.Trim();
        product.SKU = normalizedSku;
        product.UnitPrice = unitPrice;
        product.LowStockThreshold = lowStockThreshold;
        return true;
    }

    public bool Delete(int id)
    {
        var product = GetById(id);
        return product is not null && _products.Remove(product);
    }

    public bool ChangeStock(int id, int amount, bool stockIn)
    {
        var product = GetById(id);
        if (product is null || amount <= 0)
            return false;

        if (!stockIn && product.Quantity < amount)
            throw new InvalidOperationException("Not enough stock for this operation.");

        product.Quantity += stockIn ? amount : -amount;
        return true;
    }

    public IEnumerable<Product> GetLowStockProducts() =>
        _products.Where(p => p.Quantity <= p.LowStockThreshold);

    private static void ValidateProduct(string name, string sku, decimal unitPrice, int lowStockThreshold)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Price cannot be negative.");
        if (lowStockThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(lowStockThreshold),
                "Low-stock threshold cannot be negative.");
    }
}
