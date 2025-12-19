# Chouette Finance - MongoDB Setup Script
# This script helps verify and configure your MongoDB installation

Write-Host "🦉 Chouette Finance - MongoDB Setup Wizard" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if MongoDB is installed
function Test-MongoDBInstalled {
    try {
        $mongoService = Get-Service -Name "MongoDB" -ErrorAction SilentlyContinue
        return $null -ne $mongoService
    } catch {
        return $false
    }
}

# Function to check if MongoDB is running
function Test-MongoDBRunning {
    try {
        $mongoService = Get-Service -Name "MongoDB" -ErrorAction SilentlyContinue
        return $mongoService.Status -eq "Running"
    } catch {
        return $false
    }
}

# Function to test MongoDB connection
function Test-MongoDBConnection {
    param([string]$ConnectionString = "mongodb://localhost:27017")
    
    try {
        # Try to connect using mongosh
        $result = & mongosh $ConnectionString --eval "db.adminCommand('ping')" --quiet 2>&1
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

# Step 1: Check MongoDB Installation
Write-Host "Step 1: Checking MongoDB Installation..." -ForegroundColor Yellow

if (Test-MongoDBInstalled) {
    Write-Host "✓ MongoDB is installed" -ForegroundColor Green
} else {
    Write-Host "✗ MongoDB is not installed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install MongoDB from: https://www.mongodb.com/try/download/community" -ForegroundColor Yellow
    Write-Host "Choose 'Complete' installation and install as a Windows Service" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit
}

# Step 2: Check if MongoDB is running
Write-Host ""
Write-Host "Step 2: Checking MongoDB Service..." -ForegroundColor Yellow

if (Test-MongoDBRunning) {
    Write-Host "✓ MongoDB service is running" -ForegroundColor Green
} else {
    Write-Host "✗ MongoDB service is not running" -ForegroundColor Red
    Write-Host "Attempting to start MongoDB service..." -ForegroundColor Yellow
    
    try {
        Start-Service -Name "MongoDB"
        Start-Sleep -Seconds 3
        
        if (Test-MongoDBRunning) {
            Write-Host "✓ MongoDB service started successfully" -ForegroundColor Green
        } else {
            Write-Host "✗ Failed to start MongoDB service" -ForegroundColor Red
            Write-Host "Try running this script as Administrator" -ForegroundColor Yellow
            Read-Host "Press Enter to exit"
            exit
        }
    } catch {
        Write-Host "✗ Error starting MongoDB: $_" -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit
    }
}

# Step 3: Test MongoDB Connection
Write-Host ""
Write-Host "Step 3: Testing MongoDB Connection..." -ForegroundColor Yellow

$connectionString = "mongodb://localhost:27017"

# Check if mongosh is available
$mongoshPath = Get-Command mongosh -ErrorAction SilentlyContinue

if ($null -eq $mongoshPath) {
    Write-Host "⚠ mongosh (MongoDB Shell) not found in PATH" -ForegroundColor Yellow
    Write-Host "Attempting to find MongoDB installation..." -ForegroundColor Yellow
    
    # Try common MongoDB installation paths
    $commonPaths = @(
        "C:\Program Files\MongoDB\Server\7.0\bin",
        "C:\Program Files\MongoDB\Server\6.0\bin",
        "C:\Program Files\MongoDB\Server\5.0\bin"
    )
    
    foreach ($path in $commonPaths) {
        if (Test-Path "$path\mongosh.exe") {
            $env:Path += ";$path"
            Write-Host "✓ Found MongoDB at: $path" -ForegroundColor Green
            break
        }
    }
}

# Try to connect
Write-Host "Connecting to: $connectionString" -ForegroundColor Cyan

try {
    $testConnection = & mongosh $connectionString --eval "db.adminCommand('ping').ok" --quiet 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Successfully connected to MongoDB!" -ForegroundColor Green
    } else {
        Write-Host "✗ Could not connect to MongoDB" -ForegroundColor Red
        Write-Host "Error: $testConnection" -ForegroundColor Red
    }
} catch {
    Write-Host "⚠ Could not test connection (mongosh may not be installed)" -ForegroundColor Yellow
    Write-Host "MongoDB service is running, so the application should work" -ForegroundColor Yellow
}

# Step 4: Check/Create Database
Write-Host ""
Write-Host "Step 4: Database Configuration..." -ForegroundColor Yellow

try {
    $dbCheck = & mongosh $connectionString --eval "use BudgetDB; db.stats()" --quiet 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ BudgetDB database is accessible" -ForegroundColor Green
    } else {
        Write-Host "ℹ BudgetDB will be created on first application run" -ForegroundColor Cyan
    }
} catch {
    Write-Host "ℹ Database will be created automatically by the application" -ForegroundColor Cyan
}

# Step 5: Check Application Configuration
Write-Host ""
Write-Host "Step 5: Checking Application Configuration..." -ForegroundColor Yellow

$appsettingsPath = ".\appsettings.json"

if (Test-Path $appsettingsPath) {
    Write-Host "✓ appsettings.json found" -ForegroundColor Green
    
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $configuredConnection = $appsettings.ConnectionStrings.BudgetDB
    
    Write-Host "Configured connection string: $configuredConnection" -ForegroundColor Cyan
    
    if ($configuredConnection -eq "mongodb://localhost:27017") {
        Write-Host "✓ Using default local MongoDB configuration" -ForegroundColor Green
    } else {
        Write-Host "ℹ Using custom MongoDB configuration" -ForegroundColor Cyan
    }
} else {
    Write-Host "⚠ appsettings.json not found in current directory" -ForegroundColor Yellow
    Write-Host "Make sure you're running this script from the project root" -ForegroundColor Yellow
}

# Step 6: Summary and Next Steps
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setup Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$mongoInstalled = Test-MongoDBInstalled
$mongoRunning = Test-MongoDBRunning

if ($mongoInstalled -and $mongoRunning) {
    Write-Host "✓ MongoDB is ready!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Run: dotnet restore" -ForegroundColor White
    Write-Host "2. Run: dotnet build" -ForegroundColor White
    Write-Host "3. Run: dotnet run" -ForegroundColor White
    Write-Host ""
    Write-Host "Default Admin Account:" -ForegroundColor Yellow
    Write-Host "  Username: admin" -ForegroundColor White
    Write-Host "  Email: admin@example.com" -ForegroundColor White
    Write-Host "  Password: Admin#123" -ForegroundColor White
    Write-Host ""
    Write-Host "The application will automatically:" -ForegroundColor Cyan
    Write-Host "  • Create the BudgetDB database" -ForegroundColor White
    Write-Host "  • Initialize all collections" -ForegroundColor White
    Write-Host "  • Create the default admin user" -ForegroundColor White
    Write-Host ""
    
    # Ask if user wants to run the application
    $runApp = Read-Host "Would you like to run the application now? (Y/N)"
    
    if ($runApp -eq "Y" -or $runApp -eq "y") {
        Write-Host ""
        Write-Host "Starting application..." -ForegroundColor Green
        Write-Host ""
        dotnet run
    }
} else {
    Write-Host "✗ MongoDB setup incomplete" -ForegroundColor Red
    Write-Host "Please resolve the issues above and run this script again" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "For more information, see README.md" -ForegroundColor Cyan
Write-Host ""
