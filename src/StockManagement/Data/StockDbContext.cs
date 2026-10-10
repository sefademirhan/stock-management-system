using Microsoft.EntityFrameworkCore;
using StockManagement.Models;

namespace StockManagement.Data;

public class StockDbContext(DbContextOptions<StockDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.SKU).HasMaxLength(64).IsRequired();
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.Property(p => p.UnitPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("StockMovements");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.ProductSku).HasMaxLength(64).IsRequired();
            entity.Property(m => m.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Reason).HasMaxLength(20).IsRequired();
            entity.HasIndex(m => m.ProductId);
        });
    }
}
