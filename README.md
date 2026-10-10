# Stock Management System

[![.NET build and tests](https://github.com/sefademirhan/stock-management-system/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sefademirhan/stock-management-system/actions/workflows/dotnet.yml)

A beginner-friendly **C# / .NET 8** console application for basic inventory management. This project demonstrates object-oriented programming, validation, and automated unit tests.

## What works
- Create, view, update and delete products
- Track stock in and stock out
- Reject duplicate stock codes (SKU) and negative stock quantities
- Show items at or below a configurable low-stock threshold
- Automated xUnit tests for the inventory service

## Getting started on Windows
1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Download **Code → Download ZIP** on GitHub, extract it, and open **PowerShell** in the folder containing this README.
3. Run the following command **without typing `bash`**:

```powershell
dotnet run --project src/StockManagement/StockManagement.csproj
```

Choose `2` to add a product, `1` to list products, `5` or `6` to change stock, and `7` for low-stock items. Choose `0` to exit.

## Run automated tests
```powershell
dotnet test tests/StockManagement.Tests/StockManagement.Tests.csproj
```

## Current limitation and roadmap
**Data is stored in memory only. Closing the program clears all products.**

- [x] Product management and stock movements
- [x] Low-stock alerts and input validation
- [x] Unit tests and GitHub Actions checks
- [ ] Entity Framework Core + SQL Server persistence
- [ ] Stock movement history
- [ ] More tests and reporting

This project is a learning/portfolio application, not a production inventory service.
