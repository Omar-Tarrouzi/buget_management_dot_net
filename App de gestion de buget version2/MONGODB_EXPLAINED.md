# MongoDB Setup Status for Chouette Finance

## ✅ Current Status

**MongoDB Service:** ✓ Running
**Database:** BudgetDB (will be created automatically)
**Connection:** mongodb://localhost:27017

## 📋 Your MongoDB Setup Explained

### What is MongoDB?

MongoDB is a **NoSQL database** that stores data in flexible, JSON-like documents. Unlike traditional SQL databases (like SQL Server or MySQL), MongoDB:

- **Stores documents** instead of rows in tables
- **Flexible schema** - each document can have different fields
- **Scalable** - easily handles large amounts of data
- **Fast** - optimized for read/write operations
- **Native JSON** - works naturally with .NET objects

### Why MongoDB for This App?

1. **Flexible Data Model** - Budget categories, transactions, and wallets can have varying structures
2. **Easy Development** - No need to create complex migration scripts
3. **Performance** - Fast queries for financial data
4. **Scalability** - Can grow from personal use to multi-user system
5. **Modern Stack** - Popular choice for .NET Core applications

## 🏗️ How It Works in Your Application

### 1. Connection Setup (Program.cs)

```csharp
// MongoDB connection string from appsettings.json
var mongoConnectionString = "mongodb://localhost:27017";
var mongoDbName = "BudgetDB";

// Configure Entity Framework to use MongoDB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMongoDB(mongoConnectionString, mongoDbName));
```

**What this does:**
- Connects to MongoDB running on your local machine (localhost) on port 27017
- Uses a database named "BudgetDB"
- Configures Entity Framework Core to work with MongoDB

### 2. Database Context (ApplicationDbContext.cs)

```csharp
public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    // ... other collections
}
```

**What this does:**
- Defines all the "collections" (similar to tables in SQL)
- Each `DbSet<T>` becomes a collection in MongoDB
- Collections are created automatically when first accessed

### 3. Data Models with MongoDB Attributes

```csharp
public class Wallet
{
    [BsonId]  // Marks this as the primary key
    [BsonRepresentation(BsonType.ObjectId)]  // Stores as MongoDB ObjectId
    public string? WalletId { get; set; }
    
    public decimal? Balance { get; set; }
    public string UserId { get; set; }
}
```

**MongoDB Attributes Explained:**

- **`[BsonId]`** - Marks the primary key field (like `[Key]` in SQL)
- **`[BsonRepresentation(BsonType.ObjectId)]`** - Stores the ID as MongoDB's special ObjectId type
- **`[BsonIgnore]`** - Excludes a property from being saved to MongoDB
- **`[BsonElement("name")]`** - Maps property to a different field name in MongoDB

### 4. Collections Created

When you run the application, MongoDB automatically creates these collections:

| Collection | Purpose | Example Document |
|------------|---------|------------------|
| **Users** | User accounts | `{ "_id": "...", "UserName": "admin", "Email": "admin@example.com" }` |
| **Wallets** | User wallets | `{ "_id": "...", "UserId": "...", "Balance": 1000.50 }` |
| **Transactions** | Financial transactions | `{ "_id": "...", "Amount": 50.00, "Type": "Expense", "Date": "2025-12-15" }` |
| **Categories** | Transaction categories | `{ "_id": "...", "Nom": "Groceries", "UserId": "..." }` |
| **Budgets** | Monthly budgets | `{ "_id": "...", "Month": 12, "Year": 2025, "PlannedAmount": 2000 }` |

## 🔄 Data Flow Example

### Creating a Transaction

1. **User Action:** User fills out "Create Transaction" form
2. **Controller:** `TransactionController.Create()` receives the data
3. **Entity Framework:** Converts C# object to MongoDB document
4. **MongoDB Driver:** Sends insert command to MongoDB
5. **MongoDB:** Stores document in "Transactions" collection
6. **Response:** Returns success, page redirects to transaction list

```csharp
// In TransactionController.cs
var transaction = new Transaction {
    Amount = 50.00m,
    Description = "Groceries",
    Type = "Expense",
    Date = DateTime.Now,
    UserId = currentUserId,
    CategoryId = categoryId
};

_context.Transactions.Add(transaction);  // Prepare to add
await _context.SaveChangesAsync();       // Save to MongoDB
```

**In MongoDB, this becomes:**
```json
{
  "_id": ObjectId("67a1b2c3d4e5f6789abcdef0"),
  "Amount": 50.00,
  "Description": "Groceries",
  "Type": "Expense",
  "Date": ISODate("2025-12-15T22:00:00Z"),
  "UserId": "507f1f77bcf86cd799439011",
  "CategoryId": "507f191e810c19729de860ea"
}
```

## 🎯 Key Concepts

### ObjectId
- MongoDB's unique identifier format
- 12-byte value: timestamp + machine ID + process ID + counter
- Example: `507f1f77bcf86cd799439011`
- Automatically generated if not provided

### Collections vs Tables
- **SQL Table:** Fixed columns, all rows have same structure
- **MongoDB Collection:** Flexible documents, each can have different fields

### Documents vs Rows
- **SQL Row:** `| ID | Name | Amount |`
- **MongoDB Document:** `{ "_id": "...", "Name": "...", "Amount": ... }`

### No Joins (Mostly)
- MongoDB doesn't have traditional SQL joins
- Instead, you either:
  - **Embed** related data in the same document
  - **Reference** by storing IDs and querying separately
  - Use **aggregation pipeline** for complex queries

## 🔍 Viewing Your Data

### Option 1: MongoDB Compass (Recommended)

1. Download from: https://www.mongodb.com/products/compass
2. Connect to: `mongodb://localhost:27017`
3. Browse `BudgetDB` database
4. View collections visually

### Option 2: MongoDB Shell (mongosh)

```bash
# Connect
mongosh

# Switch to your database
use BudgetDB

# View all collections
show collections

# Query users
db.Users.find()

# Query transactions
db.Transactions.find().limit(10)

# Count documents
db.Transactions.countDocuments()
```

### Option 3: Application Logs

The application logs database operations to the console when running in Development mode.

## 🔐 Security Notes

### Current Setup (Development)
- **No authentication** - MongoDB accepts all connections from localhost
- **No encryption** - Data transmitted in plain text
- **Default admin user** - Username: admin, Password: Admin#123

### Production Recommendations
1. **Enable authentication** in MongoDB
2. **Use strong passwords**
3. **Enable SSL/TLS** for connections
4. **Restrict network access** (firewall rules)
5. **Regular backups**
6. **Use MongoDB Atlas** for cloud hosting with built-in security

## 📊 Performance Considerations

### Indexes
MongoDB uses indexes to speed up queries. The application should create indexes on:
- `UserId` fields (most queries filter by user)
- `Date` fields (for sorting transactions)
- `Email` and `UserName` (for login)

### Query Optimization
- Use `.AsNoTracking()` for read-only queries
- Limit result sets with `.Take()`
- Project only needed fields

### Connection Pooling
The MongoDB driver automatically manages connection pooling - you don't need to worry about opening/closing connections.

## 🆘 Troubleshooting

### "Unable to connect to MongoDB"
**Check:** Is MongoDB service running?
```powershell
Get-Service MongoDB
```
**Fix:** Start the service
```powershell
net start MongoDB
```

### "Database not found"
**This is normal!** MongoDB creates databases and collections automatically when you first write data.

### "Slow queries"
**Check:** Are indexes created?
**Fix:** Add indexes in `ApplicationDbContext.OnModelCreating()`

### "Data not showing up"
**Check:** Did you call `SaveChangesAsync()`?
**Check:** Are you querying the right collection?
**Check:** Is the data filtered by UserId?

## 📚 Learning Resources

- **MongoDB University:** https://university.mongodb.com/ (Free courses)
- **MongoDB .NET Driver Docs:** https://www.mongodb.com/docs/drivers/csharp/
- **Entity Framework Core:** https://docs.microsoft.com/ef/core/
- **MongoDB Compass:** Visual database explorer

## 🚀 Next Steps

1. ✅ MongoDB is installed and running
2. ✅ Application is configured to use MongoDB
3. ✅ Collections will be created automatically
4. ⏭️ Run the application: `dotnet run`
5. ⏭️ Login with admin account
6. ⏭️ Start creating categories and transactions!

## 💡 Quick Tips

- **Backup regularly:** Use `mongodump` command
- **Monitor performance:** Use MongoDB Compass
- **Check logs:** Located in `C:\Program Files\MongoDB\Server\7.0\log\`
- **Development:** Use MongoDB Compass to inspect data
- **Production:** Consider MongoDB Atlas for managed hosting

---

**Your MongoDB is ready! Run `dotnet run` to start the application.**
