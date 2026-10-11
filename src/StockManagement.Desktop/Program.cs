using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Services;

namespace StockManagement.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var customConnection = Environment.GetEnvironmentVariable("STOCK_DB_CONNECTION");
        // Prefer the user's working SQL Server LocalDB demo instance when present.
        var connectionStrings = string.IsNullOrWhiteSpace(customConnection)
            ? new[]
            {
                @"Server=(localdb)\StockDemo;Database=StockManagementDb;Integrated Security=True;TrustServerCertificate=True;",
                @"Server=(localdb)\MSSQLLocalDB;Database=StockManagementDb;Integrated Security=True;TrustServerCertificate=True;"
            }
            : new[] { customConnection };

        StockDbContext? db = null;
        var errors = new List<string>();

        foreach (var connectionString in connectionStrings)
        {
            try
            {
                var options = new DbContextOptionsBuilder<StockDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;
                var candidate = new StockDbContext(options);
                candidate.Database.EnsureCreated();
                db = candidate;
                break;
            }
            catch (Exception ex)
            {
                errors.Add(ex.GetBaseException().Message);
            }
        }

        if (db is null)
        {
            MessageBox.Show(
                "SQL Server LocalDB bağlantısı kurulamadı.\n\n" +
                string.Join("\n", errors.Distinct()) +
                "\n\nPowerShell'de 'sqllocaldb start StockDemo' komutunu deneyin.",
                "Veritabanı bağlantı hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using (db)
        {
            Application.Run(new InventoryForm(new InventoryService(db)));
        }
    }
}
