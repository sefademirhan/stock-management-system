namespace StockManagement.Models;

/// <summary>A permanent snapshot of a stock adjustment.</summary>
public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
