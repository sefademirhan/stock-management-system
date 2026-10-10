# Stock Management System

[![.NET build and tests](https://github.com/sefademirhan/stock-management-system/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sefademirhan/stock-management-system/actions/workflows/dotnet.yml)

A practical **C# / .NET 8 console inventory application**, now using **Entity Framework Core + SQL Server LocalDB** to save products permanently.

## Features

- Add, list, edit, and remove products
- Store products in SQL Server LocalDB between application runs
- Track stock-in and stock-out, prevent negative stock
- View low-stock alerts
- Preserve a history of initial quantities, stock-in, and stock-out, even when products are deleted
- Validate duplicate SKU codes (case-insensitive) and invalid input
- Automated xUnit tests using SQLite in-memory databases (no SQL Server required in CI)

## Requirements on Windows

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server Express LocalDB, with the standard instance `MSSQLLocalDB`

Run this in **PowerShell** to see installed LocalDB instances:

```powershell
sqllocaldb info
```

If `MSSQLLocalDB` is listed, you do not need to reinstall it. Optionally start the instance with `sqllocaldb start MSSQLLocalDB`.

## Run the application

1. Download the latest ZIP from GitHub (**Code → Download ZIP**) and extract it.
2. Open **PowerShell** in the extracted folder that contains `README.md` and `src`.
3. Run this command (do not type the word `bash`):

```powershell
dotnet run --project src/StockManagement/StockManagement.csproj
```

The app will create a **separate database named `StockManagementDb`** in `(localdb)\MSSQLLocalDB` on its first run. It does not touch other LocalDB instances or databases.

### Try it

1. Select `2` to add an item (example: name `Keyboard`, SKU `KEY-001`, price `250`, stock `10`, threshold `3`).
2. Select `1` to verify the product appears.
3. Select `5` to increase stock or `6` to reduce stock.
4. Select `8` to view the movement history.
5. Select `0` to exit. Restart with the same `dotnet run` command, then select `1` again. **Your product will still be there.**

If startup reports a SQL Server connection error, check `sqllocaldb info` and try `sqllocaldb start MSSQLLocalDB`.

## Run automated tests

```powershell
dotnet test tests/StockManagement.Tests/StockManagement.Tests.csproj
```

Tests use an isolated SQLite database in memory and do not modify your SQL Server LocalDB.

## Configuration

By default, the application connects to:

```text
Server=(localdb)\MSSQLLocalDB;Database=StockManagementDb;Trusted_Connection=True;TrustServerCertificate=True;
```

For another SQL Server instance, override the connection string via the `STOCK_DB_CONNECTION` environment variable.

## Limitations / roadmap

- [x] Product management, stock changes and low-stock alerts
- [x] SQL Server LocalDB persistent storage
- [x] Independent stock movement history
- [x] Database-backed unit tests and GitHub Actions
- [ ] EF Core migrations and schema versioning
- [ ] More reports, transaction/concurrency handling and end-to-end tests

**Note:** This educational version creates the schema using EF Core `EnsureCreated()`. That is fine for a fresh demo database, but does not support schema migrations. Use EF Core migrations for future schema updates. Products created in the earlier *in-memory* version were not saved and cannot be recovered after that version was closed.

This project is a learning/portfolio application, not a production-ready inventory system.
