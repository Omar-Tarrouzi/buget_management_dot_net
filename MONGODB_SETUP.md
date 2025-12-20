# MongoDB Setup Guide

This guide explains how to set up, link, and use MongoDB for the Budget Management Application.

## 1. Prerequisites (Setup on your machine)

Before running the application, you need a local MongoDB server.

1.  **Download MongoDB Community Server**:
    *   Go to [MongoDB Try Community](https://www.mongodb.com/try/download/community).
    *   Download and install the **MSI** package for Windows.
    *   During installation, choose "Run Service as Network Service user" (default).
    *   **Recommended**: Check the box to install **MongoDB Compass** (the GUI to view your data).

2.  **Verify It's Running**:
    *   Open "Services" in Windows and ensure `MongoDB` is running.
    *   Or open **MongoDB Compass** and connect to `mongodb://localhost:27017`.

## 2. Project Configuration (How it's linked)

We migrated from SQL Server to MongoDB. Here is how the project is configured.

### A. Nuget Packages
The project uses the official EF Core provider for MongoDB.
- `MongoDB.EntityFrameworkCore`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (for User/Role management)

### B. Connection String
In `appsettings.json`, the connection string points to your local instance.

```json
"ConnectionStrings": {
  "BudgetDB": "mongodb://localhost:27017"
}
```

The database name `BudgetDB` is defined in `Program.cs`:
```csharp
var mongoConnectionString = builder.Configuration.GetConnectionString("BudgetDB");
var mongoDbName = "BudgetDB";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMongoDB(mongoConnectionString, mongoDbName));
```

### C. Database Context (`ApplicationDbContext.cs`)
Unlike SQL tables, MongoDB uses **Collections**. We mapped them explicitly:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Mapping Classes to MongoDB Collections
    modelBuilder.Entity<Wallet>().ToCollection("Wallets");
    modelBuilder.Entity<Transaction>().ToCollection("Transactions");
    // ... etc
}
```

## 3. How We Use It (Code Changes)

### A. String IDs (ObjectId)
MongoDB uses unique 24-character strings (ObjectIds) instead of auto-incrementing Integers (1, 2, 3).
*   **Old**: `public int TransactionId { get; set; }`
*   **New**: `public string? TransactionId { get; set; }`
    *   Decorated with `[BsonId]` and `[BsonRepresentation(BsonType.ObjectId)]`.

### B. Handling Relationships
MongoDB is a **Document Database**, not a Relational Database.
*   We still use `WalletId`, `CategoryId` to link data.
*   However, "Join" operations (like `.Include(t => t.Wallet)`) might be limited.
*   **Important**: When adding new items, we often need to manually generate the ID if it's referenced immediately:
    ```csharp
    transaction.TransactionId = ObjectId.GenerateNewId().ToString();
    ```

### C. Migrations
There are **no migrations** (no `Add-Migration`).
*   MongoDB is schema-less.
*   `context.Database.EnsureCreated()` in `Program.cs` automatically creates the database and collections if they don't exist.

## 4. How to Run

1.  Clone the repository.
2.  Ensure MongoDB Service is running locally.
3.  Open the solution in Visual Studio or VS Code.
4.  Run the application (`F5` or `dotnet run`).
    *   The app will automatically create the `BudgetDB` database on the first run.
    *   It also creates a default admin user if one doesn't exist (check logs for credentials).
