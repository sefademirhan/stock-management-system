using StockManagement.Models;

namespace StockManagement.Services;

public class InventoryService
{
    private readonly List<Product> _products = [];
    private int _nextId = 1;

    public IReadOnlyList<Product> GetAll() => _products;

    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

    public Product Add(string name, string sku, decimal unitPrice, int quantity, int lowStockThreshold)
    {
        if (_products.Any(p => p.SKU.Equals(sku, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A product with this SKU already exists.");

        var product = new Product
        {
            Id = _nextId++,
            Name = name.Trim(),
            SKU = sku.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity,
            LowStockThreshold = lowStockThreshold
        };

        _products.Add(product);
        return product;
    }

    public bool Update(int id, string name, string sku, decimal unitPrice, int lowStockThreshold)
    {
        var product = GetById(id);
        if (product is null)
            return false;

        if (_products.Any(p => p.Id != id && p.SKU.Equals(sku, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Another product already uses this SKU.");

        product.Name = name.Trim();
        product.SKU = sku.Trim();
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
}
