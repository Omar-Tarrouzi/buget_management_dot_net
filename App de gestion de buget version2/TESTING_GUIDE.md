# 🧪 Testing and Running Guide - Chouette Finance

## 🚀 Quick Start - Run the Application

### Method 1: Simple Run (Recommended for First Time)

```powershell
# Navigate to project directory
cd "c:\Users\loukh\source\repos\budgetManagement-DotNet\App de gestion de buget version2"

# Run the application
dotnet run
```

**What to expect:**
- Application will compile
- MongoDB connection will be established
- Database and collections will be created
- Default admin user will be created
- Server will start on HTTPS and HTTP ports
- You'll see output like:
  ```
  info: Microsoft.Hosting.Lifetime[14]
        Now listening on: https://localhost:7xxx
        Now listening on: http://localhost:5xxx
  ```

### Method 2: Watch Mode (Auto-reload on changes)

```powershell
# Run with auto-reload
dotnet watch run
```

**Benefits:**
- Automatically reloads when you change code
- Great for development
- Saves time during testing

### Method 3: Visual Studio

1. Open `App de gestion de buget version2.csproj` in Visual Studio
2. Press **F5** or click **▶ Start Debugging**
3. Or press **Ctrl+F5** for **Start Without Debugging**

## 🌐 Accessing the Application

Once running, open your browser to:

**HTTPS (Recommended):**
```
https://localhost:7xxx
```

**HTTP:**
```
http://localhost:5xxx
```

> **Note:** Replace `xxx` with the actual port number shown in your console

**First-time HTTPS warning:**
- You may see a security warning about the SSL certificate
- This is normal for development
- Click "Advanced" → "Proceed to localhost"

## 🔐 Login Credentials

### Default Admin Account

```
Username: admin
Email: admin@example.com
Password: Admin#123
```

⚠️ **Important:** This account is created automatically on first run!

## ✅ Testing Checklist

### 1. Initial Setup Test

```powershell
# Step 1: Verify MongoDB is running
Get-Service MongoDB

# Expected output: Status = Running

# Step 2: Test MongoDB connection
mongosh --eval "db.adminCommand('ping')" --quiet

# Expected output: { ok: 1 }

# Step 3: Clean build
dotnet clean
dotnet restore
dotnet build

# Expected: Build succeeded. 0 Error(s)
```

### 2. First Run Test

```powershell
# Run the application
dotnet run
```

**Check console for:**
- ✓ `Now listening on: https://localhost:xxxx`
- ✓ `Application started. Press Ctrl+C to shut down.`
- ✓ No error messages
- ✓ MongoDB connection successful
- ✓ "Creating default admin user" message

### 3. Database Verification Test

**Option A: Using MongoDB Shell**
```bash
# Connect to MongoDB
mongosh

# Switch to BudgetDB
use BudgetDB

# List all collections
show collections

# Expected output:
# Categories
# Budgets
# Transactions
# Users
# Wallets
# ... (other collections)

# Check if admin user was created
db.Users.findOne({ UserName: "admin" })

# Expected: Should return the admin user document

# Count collections
db.Users.countDocuments()
db.Categories.countDocuments()
db.Transactions.countDocuments()
```

**Option B: Using MongoDB Compass**
1. Open MongoDB Compass
2. Connect to `mongodb://localhost:27017`
3. Click on `BudgetDB` database
4. Verify collections are created
5. Click on `Users` collection
6. Verify admin user exists

### 4. Web Interface Test

#### Test 1: Login Page
1. Navigate to `https://localhost:xxxx`
2. You should see the home page
3. Click "Se connecter" (Login) or navigate to `/Identity/Account/Login`
4. **Verify:** Login form appears with email and password fields

#### Test 2: Login Functionality
1. Enter credentials:
   - Email: `admin@example.com`
   - Password: `Admin#123`
2. Click "Log in"
3. **Expected:** Redirect to Wallet/Home page
4. **Verify:** You see "Hello admin!" in the navigation bar

#### Test 3: Create Category
1. Navigate to **Category** → **Create**
2. Enter category name: `Test Category`
3. Click **Create**
4. **Expected:** Redirect to category list
5. **Verify:** "Test Category" appears in the list

#### Test 4: Create Transaction
1. Navigate to **Transaction** → **Create**
2. Fill in:
   - Amount: `100`
   - Description: `Test Transaction`
   - Type: `Expense`
   - Category: `Test Category`
   - Date: Today's date
3. Click **Create**
4. **Expected:** Redirect to transaction list
5. **Verify:** Transaction appears in the list

#### Test 5: Create Budget
1. Navigate to **Budget** → **Create/Modify Budget**
2. Select current month and year
3. Enter planned amount: `1000`
4. Click **Save**
5. **Expected:** Budget created successfully
6. **Verify:** Budget appears in budget list

#### Test 6: View Reports
1. Navigate to **Reports** → **Monthly Summary**
2. **Verify:** Charts and data display correctly
3. **Expected:** See your test transaction in the report

#### Test 7: Logout
1. Click on "Hello admin!" in navigation
2. Click "Logout"
3. **Expected:** Redirect to home page
4. **Verify:** No longer logged in

## 🧪 Automated Testing Script

Save this as `test-app.ps1`:

```powershell
# Chouette Finance - Automated Test Script

Write-Host "🧪 Chouette Finance - Automated Testing" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: MongoDB Service
Write-Host "Test 1: Checking MongoDB Service..." -ForegroundColor Yellow
$mongoService = Get-Service -Name "MongoDB" -ErrorAction SilentlyContinue
if ($mongoService.Status -eq "Running") {
    Write-Host "✓ MongoDB service is running" -ForegroundColor Green
} else {
    Write-Host "✗ MongoDB service is not running" -ForegroundColor Red
    Write-Host "  Starting MongoDB..." -ForegroundColor Yellow
    Start-Service -Name "MongoDB"
    Start-Sleep -Seconds 3
}

# Test 2: MongoDB Connection
Write-Host ""
Write-Host "Test 2: Testing MongoDB Connection..." -ForegroundColor Yellow
$pingResult = & mongosh --eval "db.adminCommand('ping').ok" --quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ MongoDB connection successful" -ForegroundColor Green
} else {
    Write-Host "✗ MongoDB connection failed" -ForegroundColor Red
}

# Test 3: Project Build
Write-Host ""
Write-Host "Test 3: Building Project..." -ForegroundColor Yellow
$buildOutput = dotnet build --verbosity quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Project built successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Build failed" -ForegroundColor Red
    Write-Host $buildOutput
    exit 1
}

# Test 4: Check Configuration
Write-Host ""
Write-Host "Test 4: Checking Configuration..." -ForegroundColor Yellow
if (Test-Path ".\appsettings.json") {
    $config = Get-Content ".\appsettings.json" | ConvertFrom-Json
    $connString = $config.ConnectionStrings.BudgetDB
    Write-Host "✓ Configuration file found" -ForegroundColor Green
    Write-Host "  Connection: $connString" -ForegroundColor Cyan
} else {
    Write-Host "✗ Configuration file not found" -ForegroundColor Red
}

# Summary
Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "All tests passed! ✓" -ForegroundColor Green
Write-Host ""
Write-Host "Ready to run the application!" -ForegroundColor Yellow
Write-Host "Execute: dotnet run" -ForegroundColor White
Write-Host ""
```

## 🐛 Troubleshooting

### Issue: "Unable to connect to MongoDB"

**Solution:**
```powershell
# Check MongoDB service
Get-Service MongoDB

# Start if not running
net start MongoDB

# Test connection
mongosh --eval "db.adminCommand('ping')"
```

### Issue: "Build failed"

**Solution:**
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Issue: "Port already in use"

**Solution 1: Find and kill the process**
```powershell
# Find process using port 5000
netstat -ano | findstr :5000

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

**Solution 2: Change port in launchSettings.json**
Edit `Properties/launchSettings.json`:
```json
{
  "applicationUrl": "https://localhost:7001;http://localhost:5001"
}
```

### Issue: "Default admin user not created"

**Solution:**
```powershell
# Check if user exists
mongosh
use BudgetDB
db.Users.find()

# If no users, restart the application
# It will create the admin user automatically
```

### Issue: "HTTPS certificate error"

**Solution:**
```powershell
# Trust the development certificate
dotnet dev-certs https --trust
```

### Issue: "Page not found / 404 error"

**Check:**
- Are you using the correct URL?
- Is the application still running?
- Check console for errors

## 📊 Monitoring and Logs

### View Application Logs

The application logs to the console. Look for:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7xxx
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (123ms)
```

### View MongoDB Logs

```powershell
# Windows
Get-Content "C:\Program Files\MongoDB\Server\7.0\log\mongod.log" -Tail 50

# Monitor in real-time
Get-Content "C:\Program Files\MongoDB\Server\7.0\log\mongod.log" -Wait
```

### Enable Detailed Logging

Edit `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

## 🎯 Complete Test Workflow

```powershell
# 1. Verify MongoDB
Get-Service MongoDB
mongosh --eval "db.adminCommand('ping')"

# 2. Clean build
dotnet clean
dotnet restore
dotnet build

# 3. Run application
dotnet run

# 4. Open browser
start https://localhost:7xxx

# 5. Test login
# - Navigate to /Identity/Account/Login
# - Login with admin@example.com / Admin#123

# 6. Test features
# - Create category
# - Create transaction
# - Create budget
# - View reports

# 7. Verify database
mongosh
use BudgetDB
db.Transactions.find()
db.Categories.find()

# 8. Stop application
# Press Ctrl+C in console
```

## 🚀 Performance Testing

### Load Test (Optional)

```powershell
# Install Apache Bench (if available)
# Test homepage
ab -n 100 -c 10 https://localhost:7xxx/

# Test API endpoint
ab -n 100 -c 10 https://localhost:7xxx/Transaction/Index
```

### Database Performance

```javascript
// In mongosh
use BudgetDB

// Explain query
db.Transactions.find({ UserId: "user_id" }).explain("executionStats")

// Check indexes
db.Transactions.getIndexes()
```

## 📝 Test Data Generator (Optional)

Create sample data for testing:

```javascript
// In mongosh
use BudgetDB

// Get admin user ID
var adminUser = db.Users.findOne({ UserName: "admin" })
var userId = adminUser._id.toString()

// Create test categories
db.Categories.insertMany([
  { Nom: "Groceries", UserId: userId },
  { Nom: "Transport", UserId: userId },
  { Nom: "Entertainment", UserId: userId }
])

// Create test transactions
var groceryCategory = db.Categories.findOne({ Nom: "Groceries" })

db.Transactions.insertMany([
  {
    Amount: 50.00,
    Description: "Supermarket",
    Type: "Expense",
    Date: new Date(),
    UserId: userId,
    CategoryId: groceryCategory._id.toString()
  },
  {
    Amount: 1500.00,
    Description: "Monthly Salary",
    Type: "Income",
    Date: new Date(),
    UserId: userId
  }
])
```

## ✅ Success Criteria

Your application is working correctly if:

- ✓ MongoDB service is running
- ✓ Application builds without errors
- ✓ Application starts and listens on ports
- ✓ Database `BudgetDB` is created
- ✓ Collections are initialized
- ✓ Admin user is created
- ✓ Login works
- ✓ Can create categories
- ✓ Can create transactions
- ✓ Can create budgets
- ✓ Reports display correctly
- ✓ No errors in console

---

**Ready to test? Run: `dotnet run`** 🚀
