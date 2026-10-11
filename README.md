# Stock Management System

[![.NET build and tests](https://github.com/sefademirhan/stock-management-system/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sefademirhan/stock-management-system/actions/workflows/dotnet.yml) [![Windows desktop build](https://github.com/sefademirhan/stock-management-system/actions/workflows/windows-desktop.yml/badge.svg)](https://github.com/sefademirhan/stock-management-system/actions/workflows/windows-desktop.yml)

A practical inventory application built with **C# / .NET 8**, **Windows Forms**, **Entity Framework Core**, and **SQL Server LocalDB**. It includes both a desktop window and a console interface.

## Features

- Windows Forms desktop dashboard with Turkish labels, product table and searching
- Add, edit and delete products; stock-in and stock-out
- Highlight products at or below critical stock levels
- Review the stock movement history
- Export product data to UTF-8 CSV
- Persist products and stock movements in SQL Server LocalDB
- Maintain stock history after deleting a product
- Automated xUnit inventory tests and GitHub Actions build checks

## Download the Windows desktop executable

Prefer not to use the terminal? A self-contained **Windows x64 desktop build** is produced automatically by GitHub Actions.

1. Open [Windows desktop builds](https://github.com/sefademirhan/stock-management-system/actions/workflows/windows-desktop.yml).
2. Click the newest successful run.
3. Under **Artifacts**, download `StockManagement-Windows-x64`.
4. Extract the ZIP and open `StockManagement.Desktop.exe`.

The executable includes the .NET runtime, but **SQL Server LocalDB still needs to be installed**. The file is unsigned because this is a learning project; Windows may display an unknown-publisher warning. The downloadable artifact expires after 30 days, and later successful builds produce a fresh copy.

## Run the Windows desktop app

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and SQL Server Express LocalDB.
2. On Windows, download and extract this repository.
3. Open PowerShell in the folder containing `src` and `README.md`.
4. If using the local instance named `StockDemo`, start it if needed:

```powershell
sqllocaldb start StockDemo
dotnet run --project src/StockManagement.Desktop/StockManagement.Desktop.csproj
```

The desktop app first tries `(localdb)\\StockDemo`, then `(localdb)\\MSSQLLocalDB`, with database `StockManagementDb`. To choose a custom database, set `STOCK_DB_CONNECTION` in the same PowerShell window before launching.

## Run the console app

```powershell
$env:STOCK_DB_CONNECTION = 'Server=(localdb)\StockDemo;Database=StockManagementDb;Integrated Security=True;TrustServerCertificate=True;'
dotnet run --project src/StockManagement/StockManagement.csproj
```

Both applications use the same database when configured to the same connection string, so products entered in the console also appear in the desktop app.

## Run automated tests

```powershell
dotnet test tests/StockManagement.Tests/StockManagement.Tests.csproj
```

## Limitations

- This is an educational single-user inventory application, **not** a production multi-user warehouse platform.
- Schema is initialized with EF Core `EnsureCreated()`, not managed by migrations. Back up data before future schema changes.
- The desktop application works on **Windows only**.
- Simultaneous edits across multiple instances require additional concurrency protection.

Earlier versions kept data only in memory; those entries cannot be recovered. The current SQL-backed version stores new entries between runs.
