# MongoDB Quick Reference for Chouette Finance

## 🚀 Quick Start Commands

### Start/Stop MongoDB Service

```powershell
# Windows - Check status
Get-Service MongoDB

# Windows - Start service
net start MongoDB

# Windows - Stop service
net stop MongoDB

# Windows - Restart service
net stop MongoDB
net start MongoDB
```

### Connect to MongoDB

```bash
# Connect to local MongoDB
mongosh

# Connect to specific database
mongosh mongodb://localhost:27017/BudgetDB

# Connect with authentication
mongosh "mongodb://username:password@localhost:27017"
```

## 📊 Useful MongoDB Commands

### Database Operations

```javascript
// Show all databases
show dbs

// Switch to BudgetDB
use BudgetDB

// Show current database
db

// Show all collections
show collections

// Drop database (CAREFUL!)
db.dropDatabase()
```

### Collection Queries

```javascript
// Count all users
db.Users.countDocuments()

// Find all users
db.Users.find()

// Find user by email
db.Users.findOne({ Email: "admin@example.com" })

// Count transactions
db.Transactions.countDocuments()

// Find recent transactions (limit 10)
db.Transactions.find().sort({ Date: -1 }).limit(10)

// Find transactions by user
db.Transactions.find({ UserId: "user_id_here" })

// Find transactions by category
db.Transactions.find({ CategoryId: "category_id_here" })

// Count categories
db.Categories.countDocuments()

// Find all categories for a user
db.Categories.find({ UserId: "user_id_here" })

// Find all wallets
db.Wallets.find()

// Find wallet by user
db.Wallets.findOne({ UserId: "user_id_here" })
```

### Data Aggregation

```javascript
// Total transactions by user
db.Transactions.aggregate([
  { $group: { _id: "$UserId", total: { $sum: "$Amount" } } }
])

// Transactions by category
db.Transactions.aggregate([
  { $group: { _id: "$CategoryId", count: { $sum: 1 }, total: { $sum: "$Amount" } } }
])

// Monthly spending
db.Transactions.aggregate([
  { $match: { Type: "Expense" } },
  { $group: { 
      _id: { 
        year: { $year: "$Date" }, 
        month: { $month: "$Date" } 
      }, 
      total: { $sum: "$Amount" } 
    }
  },
  { $sort: { "_id.year": -1, "_id.month": -1 } }
])
```

### Data Modification

```javascript
// Update user email
db.Users.updateOne(
  { UserName: "admin" },
  { $set: { Email: "newemail@example.com" } }
)

// Update transaction amount
db.Transactions.updateOne(
  { _id: ObjectId("transaction_id_here") },
  { $set: { Amount: 100.50 } }
)

// Delete a transaction
db.Transactions.deleteOne({ _id: ObjectId("transaction_id_here") })

// Delete all transactions for a user (CAREFUL!)
db.Transactions.deleteMany({ UserId: "user_id_here" })
```

### Indexes

```javascript
// Show indexes on Transactions collection
db.Transactions.getIndexes()

// Create index on UserId for faster queries
db.Transactions.createIndex({ UserId: 1 })

// Create compound index
db.Transactions.createIndex({ UserId: 1, Date: -1 })

// Create index on Categories
db.Categories.createIndex({ UserId: 1, Nom: 1 })
```

## 🔍 Troubleshooting Queries

### Check Data Integrity

```javascript
// Find transactions without a category
db.Transactions.find({ CategoryId: null })

// Find transactions without a user
db.Transactions.find({ UserId: null })

// Find orphaned categories (no user)
db.Categories.find({ UserId: null })

// Check for duplicate categories
db.Categories.aggregate([
  { $group: { _id: { UserId: "$UserId", Nom: "$Nom" }, count: { $sum: 1 } } },
  { $match: { count: { $gt: 1 } } }
])
```

### Performance Monitoring

```javascript
// Database statistics
db.stats()

// Collection statistics
db.Transactions.stats()

// Current operations
db.currentOp()

// Server status
db.serverStatus()
```

## 💾 Backup & Restore

### Backup Database

```powershell
# Backup entire database
mongodump --db=BudgetDB --out=C:\Backups\MongoDB

# Backup with date
$date = Get-Date -Format "yyyy-MM-dd"
mongodump --db=BudgetDB --out="C:\Backups\MongoDB\BudgetDB_$date"

# Backup specific collection
mongodump --db=BudgetDB --collection=Transactions --out=C:\Backups\MongoDB
```

### Restore Database

```powershell
# Restore entire database
mongorestore --db=BudgetDB C:\Backups\MongoDB\BudgetDB

# Restore specific collection
mongorestore --db=BudgetDB --collection=Transactions C:\Backups\MongoDB\BudgetDB\Transactions.bson

# Drop existing data before restore
mongorestore --db=BudgetDB --drop C:\Backups\MongoDB\BudgetDB
```

### Export/Import JSON

```powershell
# Export collection to JSON
mongoexport --db=BudgetDB --collection=Transactions --out=transactions.json

# Export with query
mongoexport --db=BudgetDB --collection=Transactions --query='{"UserId":"user_id"}' --out=user_transactions.json

# Import from JSON
mongoimport --db=BudgetDB --collection=Transactions --file=transactions.json

# Import and drop existing data
mongoimport --db=BudgetDB --collection=Transactions --file=transactions.json --drop
```

## 🔐 Security

### Create Admin User

```javascript
// Switch to admin database
use admin

// Create admin user
db.createUser({
  user: "admin",
  pwd: "securePassword123",
  roles: [ { role: "userAdminAnyDatabase", db: "admin" } ]
})

// Create user for BudgetDB
use BudgetDB
db.createUser({
  user: "budgetapp",
  pwd: "appPassword123",
  roles: [ { role: "readWrite", db: "BudgetDB" } ]
})
```

### Update Connection String with Auth

```json
{
  "ConnectionStrings": {
    "BudgetDB": "mongodb://budgetapp:appPassword123@localhost:27017/BudgetDB"
  }
}
```

## 📈 Performance Tips

### Recommended Indexes

```javascript
// User-based queries
db.Users.createIndex({ Email: 1 }, { unique: true })
db.Users.createIndex({ UserName: 1 }, { unique: true })

// Transaction queries
db.Transactions.createIndex({ UserId: 1, Date: -1 })
db.Transactions.createIndex({ CategoryId: 1 })
db.Transactions.createIndex({ WalletId: 1 })

// Category queries
db.Categories.createIndex({ UserId: 1, Nom: 1 })

// Wallet queries
db.Wallets.createIndex({ UserId: 1 }, { unique: true })

// Budget queries
db.Budgets.createIndex({ UserId: 1, Year: 1, Month: 1 })
```

### Query Optimization

```javascript
// Use explain() to analyze queries
db.Transactions.find({ UserId: "user_id" }).explain("executionStats")

// Use projection to limit returned fields
db.Transactions.find(
  { UserId: "user_id" },
  { Amount: 1, Date: 1, Description: 1 }
)

// Use limit() for large result sets
db.Transactions.find({ UserId: "user_id" }).limit(100)
```

## 🛠️ Maintenance

### Compact Database

```javascript
// Compact collection to reclaim space
db.runCommand({ compact: "Transactions" })
```

### Repair Database

```powershell
# Repair database (stop MongoDB first)
net stop MongoDB
mongod --repair --dbpath "C:\Program Files\MongoDB\Server\7.0\data"
net start MongoDB
```

### Check Logs

```powershell
# View MongoDB logs (Windows)
Get-Content "C:\Program Files\MongoDB\Server\7.0\log\mongod.log" -Tail 50

# Monitor logs in real-time
Get-Content "C:\Program Files\MongoDB\Server\7.0\log\mongod.log" -Wait
```

## 🌐 MongoDB Compass (GUI)

### Connection String
```
mongodb://localhost:27017
```

### Useful Features
- Visual query builder
- Index management
- Schema analysis
- Performance monitoring
- Import/Export data
- Aggregation pipeline builder

## 📱 MongoDB Atlas (Cloud)

### Setup
1. Create account at https://www.mongodb.com/cloud/atlas
2. Create a free cluster
3. Add database user
4. Whitelist IP address (0.0.0.0/0 for development)
5. Get connection string

### Connection String Format
```
mongodb+srv://username:password@cluster0.xxxxx.mongodb.net/BudgetDB?retryWrites=true&w=majority
```

## 🆘 Common Issues

### Issue: "MongoServerError: Authentication failed"
**Solution:** Check username/password in connection string

### Issue: "MongoNetworkError: connect ECONNREFUSED"
**Solution:** Ensure MongoDB service is running

### Issue: "Database not found"
**Solution:** Database is created automatically on first write operation

### Issue: "Slow queries"
**Solution:** Add appropriate indexes using createIndex()

### Issue: "Disk space full"
**Solution:** Run compact command or mongodump/restore to reclaim space

---

**For more information:** https://www.mongodb.com/docs/manual/
