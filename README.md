# Stock Management System

A C# stock management console application built with .NET 8. This is a portfolio project that will grow in stages.

## Current features

- Add, list, update, and delete products
- Track stock-in and stock-out operations
- Prevent stock from becoming negative
- Warn when a product reaches its low-stock threshold
- Validate product codes (SKU) to avoid duplicates
- Validate user input

## Tech stack

- C#
- .NET 8
- Object-oriented programming

## Run locally

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Clone this repository.
3. Open a terminal in the repository folder and run:

```bash
dotnet run --project src/StockManagement/StockManagement.csproj
```

## Important note

This first milestone keeps products in memory, so data resets when the application closes. The next planned milestone is persistence with Entity Framework Core and SQL Server, followed by inventory movement history and automated tests.

## Roadmap

- [x] Product CRUD operations
- [x] Stock-in / stock-out
- [x] Low-stock alerts
- [ ] Save data with Entity Framework Core and SQL Server
- [ ] Record inventory movement history
- [ ] Add automated tests
