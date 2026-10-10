using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var culture = CultureInfo.GetCultureInfo("tr-TR");

// Windows SQL Server Express LocalDB. Override with STOCK_DB_CONNECTION if needed.
var connectionString = Environment.GetEnvironmentVariable("STOCK_DB_CONNECTION")
    ?? @"Server=(localdb)\MSSQLLocalDB;Database=StockManagementDb;Trusted_Connection=True;TrustServerCertificate=True;";

var options = new DbContextOptionsBuilder<StockDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var db = new StockDbContext(options);
try
{
    // Creates StockManagementDb and its tables on first run.
    // For learning purposes only: use EF Core migrations for future schema changes.
    db.Database.EnsureCreated();
}
catch (Exception ex)
{
    Console.Error.WriteLine("Unable to connect to SQL Server LocalDB.");
    Console.Error.WriteLine("Check 'sqllocaldb info' and 'sqllocaldb start MSSQLLocalDB'.");
    Console.Error.WriteLine($"Details: {ex.GetBaseException().Message}");
    return;
}

var inventory = new InventoryService(db);
Console.WriteLine("=== Stock Management System ===");
Console.WriteLine("Connected to StockManagementDb (SQL Server LocalDB).");
Console.WriteLine("Products and stock history are saved when you exit.");

while (true)
{
    Console.WriteLine("""
    
    1. List products
    2. Add product
    3. Update product details
    4. Delete product
    5. Stock in
    6. Stock out
    7. Show low-stock products
    8. Stock movement history
    0. Exit
    """);
    Console.Write("Select an option: ");
    var choice = Console.ReadLine()?.Trim();

    try
    {
        switch (choice)
        {
            case "1": ListProducts(); break;
            case "2": AddProduct(); break;
            case "3": UpdateProduct(); break;
            case "4": DeleteProduct(); break;
            case "5": ChangeStock(true); break;
            case "6": ChangeStock(false); break;
            case "7": ListLowStock(); break;
            case "8": ShowHistory(); break;
            case "0":
                Console.WriteLine("Goodbye!");
                return;
            default:
                Console.WriteLine("Invalid option. Try again.");
                break;
        }
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine($"Database error: {ex.GetBaseException().Message}");
    }
}

void ListProducts()
{
    var products = inventory.GetAll();
    if (products.Count == 0)
    {
        Console.WriteLine("No products found.");
        return;
    }

    Console.WriteLine("\nID | SKU | Name | Unit price | Quantity | Low-stock threshold");
    foreach (var p in products)
        Console.WriteLine($"{p.Id} | {p.SKU} | {p.Name} | {p.UnitPrice.ToString("C", culture)} | {p.Quantity} | {p.LowStockThreshold}");
}

void AddProduct()
{
    var name = ReadRequired("Product name: ");
    var sku = ReadRequired("SKU/code: ");
    var price = ReadDecimal("Unit price: ");
    var quantity = ReadInt("Starting quantity: ", 0);
    var threshold = ReadInt("Low-stock threshold: ", 0);
    var product = inventory.Add(name, sku, price, quantity, threshold);
    Console.WriteLine($"Product saved with ID {product.Id}.");
}

void UpdateProduct()
{
    var id = ReadInt("Product ID to update: ", 1);
    var product = inventory.GetById(id);
    if (product is null)
    {
        Console.WriteLine("Product not found.");
        return;
    }

    var name = ReadRequired($"Name ({product.Name}): ");
    var sku = ReadRequired($"SKU ({product.SKU}): ");
    var price = ReadDecimal($"Unit price ({product.UnitPrice}): ");
    var threshold = ReadInt($"Low-stock threshold ({product.LowStockThreshold}): ", 0);
    Console.WriteLine(inventory.Update(id, name, sku, price, threshold)
        ? "Product updated." : "Product not found.");
}

void DeleteProduct()
{
    var id = ReadInt("Product ID to delete: ", 1);
    Console.WriteLine(inventory.Delete(id) ? "Product deleted (history kept)." : "Product not found.");
}

void ChangeStock(bool stockIn)
{
    var id = ReadInt("Product ID: ", 1);
    var amount = ReadInt("Quantity: ", 1);
    Console.WriteLine(inventory.ChangeStock(id, amount, stockIn)
        ? (stockIn ? "Stock added." : "Stock removed.")
        : "Product not found or quantity is invalid.");
}

void ListLowStock()
{
    var products = inventory.GetLowStockProducts();
    if (products.Count == 0)
    {
        Console.WriteLine("No low-stock products.");
        return;
    }

    Console.WriteLine("Low-stock products:");
    foreach (var p in products)
        Console.WriteLine($"{p.Name} (SKU: {p.SKU}) — quantity: {p.Quantity}, threshold: {p.LowStockThreshold}");
}

void ShowHistory()
{
    var movements = inventory.GetHistory();
    if (movements.Count == 0)
    {
        Console.WriteLine("No stock movements yet.");
        return;
    }

    Console.WriteLine("Date/time | Product | Movement | Quantity");
    foreach (var m in movements)
        Console.WriteLine($"{m.OccurredAtUtc.ToLocalTime():yyyy-MM-dd HH:mm} | {m.ProductName} ({m.ProductSku}) | {m.Reason} | {m.QuantityChange:+#;-#;0}");
}

string ReadRequired(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var value = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(value))
            return value;
        Console.WriteLine("This field is required.");
    }
}

int ReadInt(string prompt, int minimum)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out var value) && value >= minimum)
            return value;
        Console.WriteLine($"Enter a whole number greater than or equal to {minimum}.");
    }
}

decimal ReadDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        if (decimal.TryParse(input, NumberStyles.Number, culture, out var value) && value >= 0)
            return value;
        if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value) && value >= 0)
            return value;
        Console.WriteLine("Enter a valid non-negative price (e.g. 125,50).");
    }
}
