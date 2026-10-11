using System.Globalization;
using System.Text;
using StockManagement.Services;

namespace StockManagement.Desktop;

public sealed class InventoryForm : Form
{
    private readonly InventoryService inventory;
    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        RowHeadersVisible = false
    };
    private readonly TextBox searchBox = new() { Width = 210, PlaceholderText = "Ürün adı veya kodu ara..." };
    private readonly Label summary = new() { AutoSize = true, Padding = new Padding(6) };
    private bool lowStockOnly;

    public InventoryForm(InventoryService service)
    {
        inventory = service;
        Text = "Stok Yönetim Paneli | Stock Management";
        MinimumSize = new Size(880, 530);
        Size = new Size(1100, 680);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 98, AutoScroll = true,
            Padding = new Padding(8), WrapContents = true
        };
        AddButton(buttons, "➕ Ürün ekle", AddProduct);
        AddButton(buttons, "✏️ Düzenle", EditProduct);
        AddButton(buttons, "🗑️ Sil", DeleteProduct);
        AddButton(buttons, "⬆ Stok gir", () => ChangeStock(true));
        AddButton(buttons, "⬇ Stok çıkar", () => ChangeStock(false));
        AddButton(buttons, "⚠ Kritik stok", ToggleLowStock);
        AddButton(buttons, "📜 Hareketler", ShowHistory);
        AddButton(buttons, "📄 CSV aktar", ExportCsv);
        AddButton(buttons, "🔄 Yenile", RefreshProducts);

        var searchPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 52,
            Padding = new Padding(12, 8, 8, 4)
        };
        searchPanel.Controls.Add(new Label { Text = "Ara:", AutoSize = true, Margin = new Padding(4, 8, 10, 4) });
        searchPanel.Controls.Add(searchBox);
        searchPanel.Controls.Add(summary);
        searchBox.TextChanged += (_, _) => RefreshProducts();

        Controls.Add(grid);
        Controls.Add(searchPanel);
        Controls.Add(buttons);
        RefreshProducts();
    }

    private static void AddButton(FlowLayoutPanel target, string text, Action onClick)
    {
        var button = new Button
        {
            Text = text, AutoSize = true, Height = 35,
            Margin = new Padding(3, 4, 3, 4), Padding = new Padding(6, 0, 6, 0)
        };
        button.Click += (_, _) => onClick();
        target.Controls.Add(button);
    }

    private int? SelectedId()
    {
        if (grid.CurrentRow?.Cells["Id"].Value is int id)
            return id;

        MessageBox.Show("Önce tablodan bir ürün seçin.", "Ürün seçimi",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }

    private void RefreshProducts()
    {
        RunAction(() =>
        {
            var all = inventory.GetAll();
            var filtered = all.Where(p => (!lowStockOnly || p.Quantity <= p.LowStockThreshold)
                && (string.IsNullOrWhiteSpace(searchBox.Text)
                    || p.Name.Contains(searchBox.Text, StringComparison.CurrentCultureIgnoreCase)
                    || p.SKU.Contains(searchBox.Text, StringComparison.CurrentCultureIgnoreCase)))
                .Select(p => new
                {
                    p.Id, p.SKU, p.Name, p.UnitPrice, p.Quantity, p.LowStockThreshold
                }).ToList();
            grid.DataSource = null;
            grid.DataSource = filtered;
            grid.Columns[nameof(StockManagement.Models.Product.Id)].HeaderText = "ID";
            grid.Columns[nameof(StockManagement.Models.Product.SKU)].HeaderText = "Ürün kodu";
            grid.Columns[nameof(StockManagement.Models.Product.Name)].HeaderText = "Ürün adı";
            grid.Columns[nameof(StockManagement.Models.Product.UnitPrice)].HeaderText = "Birim fiyat";
            grid.Columns[nameof(StockManagement.Models.Product.UnitPrice)].DefaultCellStyle.Format = "N2";
            grid.Columns[nameof(StockManagement.Models.Product.Quantity)].HeaderText = "Stok";
            grid.Columns[nameof(StockManagement.Models.Product.LowStockThreshold)].HeaderText = "Kritik sınır";
            summary.Text = $"   Toplam ürün: {all.Count}   Görüntülenen: {filtered.Count}";
            foreach (DataGridViewRow row in grid.Rows)
            {
                var amount = Convert.ToInt32(row.Cells[nameof(StockManagement.Models.Product.Quantity)].Value);
                var threshold = Convert.ToInt32(row.Cells[nameof(StockManagement.Models.Product.LowStockThreshold)].Value);
                if (amount <= threshold)
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
            }
        });
    }

    private void AddProduct()
    {
        using var dialog = new ProductDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        RunAction(() => inventory.Add(dialog.ProductName, dialog.SKU, dialog.Price,
            dialog.Quantity, dialog.Threshold));
        RefreshProducts();
    }

    private void EditProduct()
    {
        var id = SelectedId();
        if (id is null) return;
        var product = inventory.GetById(id.Value);
        if (product is null) { RefreshProducts(); return; }
        using var dialog = new ProductDialog(product);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        RunAction(() => inventory.Update(id.Value, dialog.ProductName, dialog.SKU,
            dialog.Price, dialog.Threshold));
        RefreshProducts();
    }

    private void DeleteProduct()
    {
        var id = SelectedId();
        if (id is null) return;
        if (MessageBox.Show("Ürün silinsin mi? Stok hareket geçmişi korunur.", "Silme onayı",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        RunAction(() => inventory.Delete(id.Value));
        RefreshProducts();
    }

    private void ChangeStock(bool incoming)
    {
        var id = SelectedId();
        if (id is null) return;
        var value = PromptQuantity(incoming ? "Stok girişi" : "Stok çıkışı");
        if (value is null) return;
        RunAction(() => inventory.ChangeStock(id.Value, value.Value, incoming));
        RefreshProducts();
    }

    private int? PromptQuantity(string title)
    {
        using var window = new Form
        {
            Text = title, Size = new Size(310, 165),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false, MaximizeBox = false
        };
        var number = new NumericUpDown
        {
            Minimum = 1, Maximum = 1_000_000, Value = 1,
            Location = new Point(22, 18), Width = 240
        };
        var ok = new Button { Text = "Onayla", DialogResult = DialogResult.OK, Location = new Point(22, 62) };
        var cancel = new Button { Text = "Vazgeç", DialogResult = DialogResult.Cancel, Location = new Point(112, 62) };
        window.Controls.AddRange([number, ok, cancel]);
        window.AcceptButton = ok;
        window.CancelButton = cancel;
        return window.ShowDialog(this) == DialogResult.OK ? (int)number.Value : null;
    }

    private void ToggleLowStock()
    {
        lowStockOnly = !lowStockOnly;
        RefreshProducts();
    }

    private void ShowHistory()
    {
        RunAction(() =>
        {
            var records = inventory.GetHistory().Select(m => new
            {
                Tarih = m.OccurredAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                Kod = m.ProductSku,
                Ürün = m.ProductName,
                Hareket = m.Reason,
                Miktar = m.QuantityChange
            }).ToList();
            using var window = new Form
            {
                Text = "Stok hareket geçmişi", Size = new Size(850, 500),
                StartPosition = FormStartPosition.CenterParent
            };
            window.Controls.Add(new DataGridView
            {
                Dock = DockStyle.Fill, DataSource = records,
                ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false, RowHeadersVisible = false
            });
            window.ShowDialog(this);
        });
    }

    private void ExportCsv()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "CSV dosyası (*.csv)|*.csv", FileName = "stok-listesi.csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        RunAction(() =>
        {
            var sb = new StringBuilder("Id;SKU;Ürün;Fiyat;Stok;Kritik sınır\n");
            foreach (var p in inventory.GetAll())
            {
                sb.AppendLine(string.Join(";", p.Id,
                    CsvCell(p.SKU), CsvCell(p.Name),
                    p.UnitPrice.ToString(CultureInfo.InvariantCulture),
                    p.Quantity, p.LowStockThreshold));
            }
            File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show("CSV dosyası kaydedildi.", "Aktarma tamamlandı");
        });
    }

    // Prefix formula-like cells to prevent spreadsheet formula execution when exporting CSV.
    private static string CsvCell(string value)
    {
        var escaped = value.TrimStart().StartsWith('=') || value.TrimStart().StartsWith('+')
            || value.TrimStart().StartsWith('-') || value.TrimStart().StartsWith('@')
            ? "'" + value : value;
        return "\"" + escaped.Replace("\"", "\"\"") + "\"";
    }

    private static void RunAction(Action action)
    {
        try { action(); }
        catch (Exception ex)
        {
            MessageBox.Show(ex.GetBaseException().Message, "İşlem başarısız",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
