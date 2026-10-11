using StockManagement.Models;

namespace StockManagement.Desktop;

internal sealed class ProductDialog : Form
{
    private readonly TextBox nameBox = new() { Width = 250 };
    private readonly TextBox skuBox = new() { Width = 250 };
    private readonly NumericUpDown priceBox = new()
    {
        Width = 250, DecimalPlaces = 2, Maximum = 100_000_000,
        ThousandsSeparator = true
    };
    private readonly NumericUpDown quantityBox = new()
    {
        Width = 250, Maximum = 1_000_000
    };
    private readonly NumericUpDown thresholdBox = new()
    {
        Width = 250, Maximum = 1_000_000, Value = 5
    };

    public string ProductName => nameBox.Text.Trim();
    public string SKU => skuBox.Text.Trim();
    public decimal Price => priceBox.Value;
    public int Quantity => (int)quantityBox.Value;
    public int Threshold => (int)thresholdBox.Value;

    public ProductDialog(Product? product = null)
    {
        Text = product is null ? "Ürün Ekle" : "Ürün Düzenle";
        ClientSize = new Size(420, product is null ? 330 : 285);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 10);

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(18),
            RowCount = 6, ColumnCount = 2
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(table, 0, "Ürün adı", nameBox);
        AddRow(table, 1, "Ürün kodu (SKU)", skuBox);
        AddRow(table, 2, "Birim fiyat", priceBox);

        var nextRow = 3;
        if (product is null)
            AddRow(table, nextRow++, "Başlangıç stok", quantityBox);

        AddRow(table, nextRow++, "Kritik stok sınırı", thresholdBox);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var saveButton = new Button { Text = "Kaydet", AutoSize = true };
        var cancelButton = new Button { Text = "Vazgeç", AutoSize = true, DialogResult = DialogResult.Cancel };
        saveButton.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(SKU))
            {
                MessageBox.Show("Ürün adı ve kodu boş bırakılamaz.", "Eksik bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        };
        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(cancelButton);
        table.Controls.Add(buttons, 0, nextRow);
        table.SetColumnSpan(buttons, 2);

        if (product is not null)
        {
            nameBox.Text = product.Name;
            skuBox.Text = product.SKU;
            priceBox.Value = Math.Min(priceBox.Maximum, product.UnitPrice);
            thresholdBox.Value = Math.Min(thresholdBox.Maximum, product.LowStockThreshold);
        }

        Controls.Add(table);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static void AddRow(TableLayoutPanel table, int row, string name, Control control)
    {
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        table.Controls.Add(new Label { Text = name, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        table.Controls.Add(control, 1, row);
    }
}
