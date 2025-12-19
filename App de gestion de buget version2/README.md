# 🦉 Chouette Finance - Budget Management Application

A beautiful, modern budget management application built with **ASP.NET Core 8.0** and **MongoDB**. Track your expenses, manage budgets, and achieve your financial goals with style!

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![MongoDB](https://img.shields.io/badge/MongoDB-4EA94B?logo=mongodb&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?logo=bootstrap&logoColor=white)

## ✨ Features

- 💰 **Wallet Management** - Track multiple wallets and balances
- 📊 **Transaction Tracking** - Record income and expenses with categories
- 🎯 **Budget Planning** - Set monthly budgets and track spending
- 📈 **Financial Reports** - Visualize your financial data with charts
- 🔐 **User Authentication** - Secure login with ASP.NET Core Identity
- 🎨 **Beautiful UI** - Modern glassmorphic design with animated particles
- 📱 **Responsive Design** - Works seamlessly on all devices
- 📁 **CSV Import/Export** - Import transactions from bank statements

## 🗄️ Database Architecture

This application uses **MongoDB** as its database, which provides:
- **Flexible Schema** - Easy to evolve your data model
- **High Performance** - Fast reads and writes
- **Scalability** - Grows with your needs
- **Document-Based** - Natural fit for .NET objects

### Collections Structure

The application uses the following MongoDB collections:

| Collection | Description |
|------------|-------------|
| `Users` | User accounts (ASP.NET Identity) |
| `Roles` | User roles |
| `Wallets` | User wallet information |
| `Transactions` | Financial transactions |
| `Categories` | Transaction categories |
| `Budgets` | Monthly budget plans |
| `CategoryBudgets` | Budget allocations per category |
| `Goals` | Financial goals |
| `MonthlyPayments` | Recurring monthly payments |
| `Salaries` | Salary information |
| `RecurringIncomes` | Recurring income sources |

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** or later
2. **[MongoDB Community Server](https://www.mongodb.com/try/download/community)** (version 4.4 or later)
3. **[MongoDB Compass](https://www.mongodb.com/products/compass)** (optional, for database visualization)
4. A code editor like **[Visual Studio 2022](https://visualstudio.microsoft.com/)** or **[VS Code](https://code.visualstudio.com/)**

### 📥 MongoDB Installation

#### Windows

1. **Download MongoDB**
   - Visit [MongoDB Download Center](https://www.mongodb.com/try/download/community)
   - Select "Windows" platform
   - Download the MSI installer

2. **Install MongoDB**
   - Run the installer
   - Choose "Complete" installation
   - Install as a Windows Service (recommended)
   - Install MongoDB Compass (optional but recommended)

3. **Verify Installation**
   ```powershell
   # Check if MongoDB service is running
   Get-Service MongoDB
   
   # Or start it manually
   net start MongoDB
   ```

4. **Test Connection**
   ```powershell
   # Connect to MongoDB shell
   mongosh
   
   # You should see a connection message
   # Type 'exit' to quit
   ```

#### macOS

```bash
# Install using Homebrew
brew tap mongodb/brew
brew install mongodb-community@7.0

# Start MongoDB service
brew services start mongodb-community@7.0

# Verify installation
mongosh
```

#### Linux (Ubuntu/Debian)

```bash
# Import MongoDB public key
wget -qO - https://www.mongodb.org/static/pgp/server-7.0.asc | sudo apt-key add -

# Add MongoDB repository
echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu jammy/mongodb-org/7.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-7.0.list

# Update and install
sudo apt-get update
sudo apt-get install -y mongodb-org

# Start MongoDB
sudo systemctl start mongod
sudo systemctl enable mongod

# Verify
mongosh
```

### 🔧 Application Setup

1. **Clone or Download the Repository**
   ```bash
   cd "c:\Users\loukh\source\repos\budgetManagement-DotNet\App de gestion de buget version2"
   ```

2. **Configure MongoDB Connection**
   
   The default configuration in `appsettings.json` is:
   ```json
   {
     "ConnectionStrings": {
       "BudgetDB": "mongodb://localhost:27017"
     }
   }
   ```

   **For Custom MongoDB Setup:**
   - If MongoDB is on a different host: `mongodb://your-host:27017`
   - With authentication: `mongodb://username:password@localhost:27017`
   - MongoDB Atlas (cloud): `mongodb+srv://username:password@cluster.mongodb.net/`

3. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

4. **Build the Application**
   ```bash
   dotnet build
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

6. **Access the Application**
   - Open your browser and navigate to: `https://localhost:7xxx` (check console for exact port)
   - The application will automatically:
     - Create the `BudgetDB` database
     - Initialize all collections
     - Create a default admin user

### 👤 Default Admin Account

On first run, the application creates a default admin account:

- **Username:** `admin`
- **Email:** `admin@example.com`
- **Password:** `Admin#123`

⚠️ **Important:** Change this password immediately after first login in production!

## 🏗️ Project Structure

```
App de gestion de buget version2/
├── Areas/
│   └── Identity/          # Identity pages (Login, Register)
├── Controllers/           # MVC Controllers
│   ├── BudgetController.cs
│   ├── CategoryController.cs
│   ├── TransactionController.cs
│   └── WalletController.cs
├── Data/
│   ├── ApplicationDbContext.cs    # MongoDB DbContext
│   └── CustomUserStore.cs         # Custom Identity store for MongoDB
├── Models/                # Data models
│   ├── Budget.cs
│   ├── Category.cs
│   ├── Transaction.cs
│   ├── Wallet.cs
│   └── ViewModels/
├── Services/              # Business logic
│   └── CsvService.cs
├── Views/                 # Razor views
│   ├── Budget/
│   ├── Category/
│   ├── Transaction/
│   ├── Wallet/
│   └── Shared/
├── wwwroot/              # Static files
│   ├── css/
│   │   └── site.css      # Custom styling
│   ├── js/
│   └── images/
├── Program.cs            # Application entry point
└── appsettings.json      # Configuration
```

## 🔑 Key Technologies

### Backend
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core** - ORM
- **MongoDB.EntityFrameworkCore** - MongoDB provider for EF Core
- **ASP.NET Core Identity** - Authentication & authorization
- **MongoDB.Driver** - Official MongoDB .NET driver

### Frontend
- **Bootstrap 5.3** - UI framework
- **Chart.js** - Data visualization
- **Custom CSS** - Glassmorphic design with animations
- **Google Fonts** - DM Sans & Space Grotesk typography

## 💾 MongoDB Configuration Details

### Connection String Format

The application uses the standard MongoDB connection string format:

```
mongodb://[username:password@]host[:port][/[defaultauthdb][?options]]
```

**Examples:**

```json
// Local development (default)
"BudgetDB": "mongodb://localhost:27017"

// With authentication
"BudgetDB": "mongodb://myUser:myPassword@localhost:27017"

// MongoDB Atlas (cloud)
"BudgetDB": "mongodb+srv://username:password@cluster0.xxxxx.mongodb.net/?retryWrites=true&w=majority"

// Custom port
"BudgetDB": "mongodb://localhost:27018"

// Replica set
"BudgetDB": "mongodb://host1:27017,host2:27017,host3:27017/?replicaSet=myReplicaSet"
```

### Database Initialization

The application automatically:
1. Creates the `BudgetDB` database if it doesn't exist
2. Creates all required collections on first access
3. Sets up indexes for optimal performance
4. Initializes ASP.NET Identity collections

### Data Models with MongoDB Attributes

All models use MongoDB-specific attributes:

```csharp
[BsonId]                                    // Marks the primary key
[BsonRepresentation(BsonType.ObjectId)]     // Stores as ObjectId
public string? WalletId { get; set; }

[BsonIgnore]                                // Excludes from MongoDB
public virtual ICollection<Transaction> Transactions { get; set; }
```

## 🛠️ Development

### Running in Development Mode

```bash
dotnet run --environment Development
```

### Watching for Changes

```bash
dotnet watch run
```

### Database Management

**Using MongoDB Compass:**
1. Open MongoDB Compass
2. Connect to `mongodb://localhost:27017`
3. Browse the `BudgetDB` database
4. View and edit collections

**Using mongosh (MongoDB Shell):**
```bash
# Connect to database
mongosh

# Switch to BudgetDB
use BudgetDB

# List collections
show collections

# Query users
db.Users.find()

# Query transactions
db.Transactions.find().limit(10)

# Count documents
db.Transactions.countDocuments()
```

### Backup and Restore

**Backup:**
```bash
mongodump --db=BudgetDB --out=./backup
```

**Restore:**
```bash
mongorestore --db=BudgetDB ./backup/BudgetDB
```

## 🎨 UI Features

- **Glassmorphic Cards** - Modern frosted glass effect
- **Animated Particles** - Floating money symbols background
- **Gradient Typography** - Eye-catching headers
- **Smooth Transitions** - Polished hover effects
- **Responsive Tables** - Beautiful data presentation
- **Chart Visualizations** - Interactive financial charts

## 📊 Usage Guide

### Creating Your First Budget

1. **Login** with the default admin account
2. **Create Categories** - Navigate to Category → Create
3. **Add Transactions** - Go to Transaction → Create
4. **Set Budget** - Visit Budget → Create/Modify Budget
5. **View Reports** - Check Reports for insights

### Importing Transactions

1. Navigate to **Transaction → Import CSV**
2. Upload a CSV file with columns: `Date`, `Description`, `Amount`, `Type`, `Category`
3. The system will automatically:
   - Parse the transactions
   - Create missing categories
   - Deduce categories from descriptions

## 🔒 Security

- Passwords are hashed using ASP.NET Core Identity
- HTTPS enforced in production
- CSRF protection enabled
- SQL injection prevented (NoSQL database)
- User data isolation (each user sees only their data)

## 🐛 Troubleshooting

### MongoDB Connection Issues

**Problem:** "Unable to connect to MongoDB"
```bash
# Check if MongoDB is running
Get-Service MongoDB  # Windows
brew services list   # macOS
sudo systemctl status mongod  # Linux

# Start MongoDB if stopped
net start MongoDB    # Windows
brew services start mongodb-community  # macOS
sudo systemctl start mongod  # Linux
```

**Problem:** "Authentication failed"
- Verify username/password in connection string
- Check MongoDB user permissions
- Ensure authentication is enabled in MongoDB config

### Build Errors

**Problem:** Package restore fails
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

**Problem:** Identity scaffolding issues
- Ensure `Microsoft.AspNetCore.Identity.UI` package is installed
- Check that Areas/Identity folder structure is correct

## 📝 Environment Variables

For production, use environment variables instead of hardcoding connection strings:

**Windows:**
```powershell
$env:ConnectionStrings__BudgetDB="mongodb://production-host:27017"
```

**Linux/macOS:**
```bash
export ConnectionStrings__BudgetDB="mongodb://production-host:27017"
```

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "BudgetDB": "mongodb://production-server:27017"
  }
}
```

## 🚢 Deployment

### MongoDB Atlas (Cloud)

1. Create a free cluster at [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)
2. Get your connection string
3. Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "BudgetDB": "mongodb+srv://username:password@cluster.mongodb.net/"
     }
   }
   ```

### Publishing the Application

```bash
# Publish for production
dotnet publish -c Release -o ./publish

# Run published app
cd publish
dotnet "App de gestion de buget version2.dll"
```

## 📚 Additional Resources

- [MongoDB .NET Driver Documentation](https://www.mongodb.com/docs/drivers/csharp/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [MongoDB University](https://university.mongodb.com/) - Free courses

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## 📄 License

This project is for educational purposes.

## 👨‍💻 Support

For issues and questions:
- Check the troubleshooting section
- Review MongoDB logs: `C:\Program Files\MongoDB\Server\7.0\log\mongod.log` (Windows)
- Check application logs in the console output

---

**Made with ❤️ using ASP.NET Core and MongoDB**
