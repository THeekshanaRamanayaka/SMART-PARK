# SmartPark (project)

Minimal README for the `SmartPark` app folder.

Quick run (MacCatalyst):

From the repository root (recommended) — target the solution or the app project explicitly:

```bash
# Build the solution (from repo root)
dotnet restore "Smart-Park.sln"
dotnet build "Smart-Park.sln" -f net10.0-maccatalyst

# Run the MacCatalyst app by project
dotnet run --project ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst
```

Or run from the `SmartPark` folder directly:

```bash
cd SmartPark
dotnet restore
dotnet build SmartPark.csproj -f net10.0-maccatalyst
dotnet run --project SmartPark.csproj -f net10.0-maccatalyst
```

Local API (for tests):

- Base URL: `http://127.0.0.1:5099` (set `SMARTPARK_ENABLE_LOCAL_API=false` to disable)
- Import `SmartPark.postman_collection.json` into Postman and set `base_url` to `http://127.0.0.1:5099`.

E2E helper script (repo root): `./run-e2e-ui-db-flow.sh`

Database: MySQL (see repo `docker-compose.yml`).

This file is intentionally short—see the repository root `README.md` for full documentation and testing instructions.

# Smart Parking Slot Monitoring and Management System

University project built with .NET MAUI Blazor Hybrid and Entity Framework Core.

## 📋 Project Overview

SmartPark is a comprehensive parking management system that handles parking slots, vehicle entries/exits, active occupancy monitoring, history lookup, and reporting. Built with modern .NET technologies and featuring a vibrant, user-friendly interface.

## ✨ Key Features

1. **Dashboard** - Real-time slot statistics and overview
2. **Manage Slots** - Add, edit, and delete parking slots
3. **Vehicle Entry** - Register vehicles with automatic slot assignment
4. **Active Parking** - Monitor current occupancy and record exits
5. **Parking Records** - Search and view all parking history
6. **Reports** - Generate reports for completed sessions
7. **Vehicle History** - View past parking details for any vehicle
8. **Data Persistence** - MySQL database with cross-session data retention
9. **DBeaver Compatible** - Direct MySQL database access for management

## 🛠 Technology Stack

- **.NET 10** - Latest .NET framework
- **.NET MAUI Blazor Hybrid** - Cross-platform UI framework
- **C#** - Primary programming language
- **Entity Framework Core** - ORM for database operations
- **MySQL** - Relational database engine
- **MudBlazor** - Material Design component library
- **Bootstrap CSS** - Responsive styling

## 📁 Project Structure

```text
SmartPark/
├── Models/
│   ├── ParkingSlot.cs          # Parking slot entity
│   ├── ParkingRecord.cs        # Parking session entity
│   └── AppUser.cs              # User entity
├── Data/
│   ├── SmartParkDbContext.cs   # EF Core context
│   └── SmartParkDbContextFactory.cs
├── Services/
│   ├── ParkingSlotService.cs   # Slot management logic
│   ├── ParkingRecordService.cs # Record management logic
│   └── DatabaseInitializer.cs  # DB initialization
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor    # Main app layout
│   │   └── NavMenu.razor       # Navigation menu
│   └── Pages/
│       ├── Dashboard.razor     # Dashboard page
│       ├── ManageSlots.razor   # Slot management
│       ├── VehicleEntry.razor  # Vehicle registration
│       ├── ActiveParking.razor # Active sessions
│       ├── ParkingRecords.razor# History & search
│       └── Reports.razor       # Reporting
└── MauiProgram.cs              # App configuration
```

## 🚀 Prerequisites

- **macOS** (for MacCatalyst)
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download)
- **Xcode Command Line Tools** (for macOS)
- **VS Code** or **Visual Studio** (optional)
- **DBeaver** (optional, for database management)

## Local API for Postman

When you run the app, it also starts a small localhost API on `http://127.0.0.1:5099` unless `SMARTPARK_ENABLE_LOCAL_API=false` is set.

Use that API from Postman or cURL to test the same slot and parking-record flow used by the UI.

## 📦 Setup and Installation

### 1. Navigate to Project Directory (optional)

You can run commands from the repository root or from inside the `SmartPark` folder. Examples below use explicit targets so the current working directory doesn't matter.

### 2. Restore NuGet Packages

From repo root (solution):

```bash
dotnet restore "Smart-Park.sln"
```

Or restore just the app project:

```bash
dotnet restore ./SmartPark/SmartPark.csproj
```

### Finalization Notes

- **Solution path fixed:** The solution `Smart-Park.sln` previously referenced `SmartPark\SmartPark.csproj`; I've corrected it to reference `SmartPark.csproj` so `dotnet restore "Smart-Park.sln"` works from this folder.
- **Verified:** I ran `dotnet restore`, `dotnet build -f net10.0-maccatalyst`, and `dotnet run --project ./SmartPark.csproj -f net10.0-maccatalyst` successfully on macOS (MacCatalyst). If you want, I can commit and push these changes to the repository.

### 3. Build the Project

From repo root (solution):

```bash
dotnet build "Smart-Park.sln" -f net10.0-maccatalyst
```

Or build just the app project:

```bash
dotnet build ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst
```

### 4. Run the Application

Run the MacCatalyst app by pointing `dotnet run` at the project file (works from any cwd):

```bash
dotnet run --project ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst
```

Or open the built app directly after a successful build:

```bash
open SmartPark/bin/Debug/net10.0-maccatalyst/SmartPark.app
```

On first launch, the app automatically creates the MySQL schema using `EnsureCreatedAsync`.

## 💾 Database Details

- **Type**: MySQL
- **ORM**: Entity Framework Core
- **Connection**: Configured via `SMARTPARK_MYSQL_CONNECTION`
- **Core Tables**:
  - `ParkingSlots` - Parking slot information
  - `ParkingRecords` - Parking session records
  - `AppUsers` - User accounts

### Database Persistence

The database is server-based in MySQL, ensuring data remains intact after app stop/start cycles.

### DBeaver Integration

1. Open DBeaver
2. Create new connection → MySQL
3. If you use the Docker stack, use host `127.0.0.1`, port `3307`, database `smartpark`, username `smartpark`, password `smartpark`
4. Connect and manage data

**Note**: Ensure the app and DBeaver point to the same MySQL database instance.

### Dockerized MySQL

Run the database locally in Docker:

```bash
docker compose up -d
```

The container exposes MySQL on `127.0.0.1:3307` and persists data in a named Docker volume. DBeaver can connect to it using the same credentials shown above.

To stop it later:

```bash
docker compose down
```

## 🎯 Usage Guide

### Complete Workflow

1. **Setup Parking Slots** (Manage Slots)
   - Navigate to "Manage Slots"
   - Add slots: A-01, A-02, A-03, B-01, B-02, etc.
   - View visual slot map
   - Edit or delete available slots

2. **Register Vehicle Entry** (Vehicle Entry)
   - Navigate to "Vehicle Entry"
   - Enter vehicle number (e.g., ABC-1234)
   - Enter owner name
   - Click "Register Entry"
   - System automatically assigns available slot

3. **View Past Vehicle Details** (Vehicle Entry)
   - Enter vehicle number
   - Click "View Past Details"
   - See completed parking history for that vehicle

4. **Monitor Active Parking** (Active Parking)
   - View all currently parked vehicles
   - See entry times and durations
   - Monitor slot assignments

5. **Record Vehicle Exit** (Active Parking)
   - Click "Record Exit" for departing vehicle
   - System calculates duration automatically
   - Slot becomes available immediately

6. **Search Parking History** (Parking Records)
   - View all records (active + completed)
   - Search by vehicle number or slot number
   - Filter and review parking sessions

7. **Generate Reports** (Reports)
   - View completed sessions only
   - See statistics (total, average, duration)
   - Export report data

## 🏗 Architecture

### Clean Architecture Pattern

```
┌─────────────────────────────────────┐
│     UI Layer (Blazor Pages)         │
│  Dashboard, ManageSlots, etc.       │
└──────────────┬──────────────────────┘
               │ @inject
┌──────────────▼──────────────────────┐
│   Service Layer (Business Logic)    │
│  ParkingSlotService, etc.           │
└──────────────┬──────────────────────┘
               │ uses
┌──────────────▼──────────────────────┐
│   Data Layer (EF Core Context)      │
│  SmartParkDbContext                 │
└──────────────┬──────────────────────┘
               │ persists to
┌──────────────▼──────────────────────┐
│      MySQL Database                 │
└─────────────────────────────────────┘
```

### Data Flow Examples

**Vehicle Entry Flow:**

1. User submits vehicle info → VehicleEntry page
2. Page calls `RecordVehicleEntryAsync()` → ParkingRecordService
3. Service finds first available slot → ParkingSlotService
4. New ParkingRecord created with `IsCompleted=false`
5. Slot marked as occupied
6. Changes saved to MySQL

**Vehicle Exit Flow:**

1. User clicks "Record Exit" → ActiveParking page
2. Page calls `RecordVehicleExitAsync()` → ParkingRecordService
3. Service sets ExitTime, Duration, `IsCompleted=true`
4. Related slot marked as available
5. Changes saved to MySQL

## 🔧 Useful Commands

### Run Application

Run by specifying the project path (from anywhere):

```bash
dotnet run --project ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst
```

### Build Only

Build the solution or the project explicitly:

```bash
dotnet build "Smart-Park.sln" -f net10.0-maccatalyst
# or
dotnet build ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst
```

### Clean and Rebuild

```bash
dotnet clean
dotnet build -f net10.0-maccatalyst
```

### Reset MySQL Data (Optional)

```bash
mysql -h 127.0.0.1 -P 3306 -u smartpark -p -e "DROP DATABASE IF EXISTS smartpark; CREATE DATABASE smartpark;"
```

### Local API Checks

```bash
curl http://127.0.0.1:5099/health
```

```bash
curl http://127.0.0.1:5099/api/slots
```

```bash
curl -X POST http://127.0.0.1:5099/api/slots \
   -H "Content-Type: application/json" \
   -d '{"slotNumber":"A-01","isOccupied":false}'
```

```bash
curl -X POST http://127.0.0.1:5099/api/records/entry \
   -H "Content-Type: application/json" \
   -d '{"vehicleNumber":"ABC-1234","ownerName":"Test Driver"}'
```

```bash
curl -X POST http://127.0.0.1:5099/api/records/1/exit
```

```bash
curl "http://127.0.0.1:5099/api/records/search?term=ABC"
```

```bash
curl http://127.0.0.1:5099/api/records/history/ABC-1234
```

```bash
curl http://127.0.0.1:5099/api/slots/available-count
```

```bash
curl http://127.0.0.1:5099/api/slots/occupied-count
```

## Runtime Logs

Startup, database, slot, and parking-record operations are logged at runtime. When you run the app with `dotnet run -f net10.0-maccatalyst`, watch the terminal output for `Information`, `Warning`, and `Error` events.

## Viewing Logs

- **Terminal (when running the app):** The simplest place to see runtime logs is the terminal where you started the app with `dotnet run` — the app writes `Information`/`Warning`/`Error` lines to stdout.

```bash
dotnet run --project ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst
# watch output in the same terminal
```

- **Mac system logs (MacCatalyst app):** Use Console.app and filter by process name `SmartPark`, or use the `log` command in Terminal to stream system logs for the app:

```bash
# live stream logs for the SmartPark process
log stream --style syslog --predicate 'process == "SmartPark"'

# show recent logs (last hour)
log show --style syslog --predicate 'process == "SmartPark"' --last 1h
```

- **Local API and health checks:** If you only need to check the API, query the health endpoint or API endpoints; logs related to requests and API startup appear in the terminal running the app:

```bash
curl http://127.0.0.1:5099/health
```

- **VS Code:** If you run the app from VS Code's debugger/launch, open the `Terminal` or `Debug Console` panel to see the same output.

- **Persistent logging (optional):** If you want logs written to a file, add a file logger provider in `MauiProgram.cs` or your logging configuration (e.g., `Microsoft.Extensions.Logging` with a rolling-file provider). I can add a simple file-logging setup if you'd like.

## 🐛 Troubleshooting

## Audit & Finalization

- **Audit summary:** project structure verified. Key areas: `Data/` (EF context present, `Data/Migrations/` is empty), `Sql/` (schema + seeds present), `Services/` (business logic and DB initializer), `Components/` (Blazor pages and layout), `Resources/` (assets), `Tools/` (helpers).

- **Decisions & actions to take:**
   - Decide whether to commit EF Core migrations into `Data/Migrations/` or continue shipping `Sql/init-mysql-schema.sql`. Keep them in sync.
   - Confirm `smartpark.db` purpose; remove if leftover.
   - Add a small test harness or document the absence of automated tests.
   - Optionally add file logging and CI build scripts.

- **E2E & DB:** `run-e2e-ui-db-flow.sh` seeds the DB from `Sql/` and verifies results; it requires the `mysql` CLI and a running MySQL instance (default: `127.0.0.1:3307`). `docker-compose.yml` can start a MySQL service matching defaults.

**Quick checklist (finalize):**

- [ ] Choose migrations vs SQL scripts strategy and update repo accordingly.
- [ ] Remove unused artifacts (e.g., `smartpark.db`) or document their purpose.
- [ ] Add minimal unit/integration tests or a note in docs explaining testing strategy.
- [ ] Add CI build instructions (optional): `dotnet restore && dotnet build`.
- [ ] Run the E2E flow and verify UI + DB:

```bash
# Start MySQL for local testing
docker compose up -d

# Seed DB and run verification (from project folder)
./run-e2e-ui-db-flow.sh

# Build and run the app (MacCatalyst)
dotnet build ./Smart-Park.sln -f net10.0-maccatalyst
dotnet run --project ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst
```

If you'd like, I can commit a `docs/FINALIZE.md` with this checklist, remove `smartpark.db` if it's not required, and/or add EF migration files. Tell me which you'd prefer and I'll proceed.

### Build Fails

- Run `dotnet restore`
- Then `dotnet build -f net10.0-maccatalyst`
- Ensure .NET 10 SDK is installed

### Cannot Connect to MySQL in DBeaver

- Ensure MySQL server is running
- Verify host/port/database/user/password
- Run the app once to create schema if needed

### No Vehicle History Shown

- "View Past Details" returns completed sessions only
- Active sessions are excluded from history
- Ensure vehicle has completed at least one parking session

### Xcode Errors (MacCatalyst)

- Install Xcode Command Line Tools: `xcode-select --install`
- For full Xcode: Download from App Store
- Set developer directory: `sudo xcode-select -s /Applications/Xcode.app/Contents/Developer`

### Application Won't Launch

- Check if app is already running
- Try: `open bin/Debug/net10.0-maccatalyst/SmartPark.app`
- Check Console.app for error logs

## 📊 Testing

Refer to `TESTING_QUICK_START.md` for the current MySQL desktop testing workflow.

## 📚 Documentation

- **README.md** (this file) - Complete project documentation
- **TESTING_QUICK_START.md** - MySQL end-to-end test flow

## 🎓 Project Information

- **Course**: ITE1943 - ICT Project
- **Institution**: University of Moratuwa
- **Type**: Educational project
- **Framework**: .NET MAUI Blazor Hybrid
- **Database**: MySQL with Entity Framework Core

## 🌟 Features Implemented

✅ Parking slot CRUD operations  
✅ Vehicle registration with validation  
✅ Automatic slot assignment (first available)  
✅ Real-time occupancy monitoring  
✅ Vehicle entry recording with timestamp  
✅ Vehicle exit recording with duration calculation  
✅ Automatic slot status updates  
✅ All-records view (active + completed)  
✅ Search by vehicle number or slot number  
✅ Completed-session reporting  
✅ Vehicle history lookup  
✅ Data persistence across sessions  
✅ DBeaver database management support  
✅ Modern, vibrant UI with MudBlazor  
✅ Responsive design  
✅ Real-time statistics dashboard  

## 🔐 Data Integrity

- **Slot Occupancy**: Total Slots = Available + Occupied (always)
- **Occupied Slots**: Cannot be edited or deleted
- **Slot Release**: Becomes available immediately after exit
- **Record Completeness**: Active records have entry time only; completed records have both entry and exit times
- **Duration Calculation**: Automatic for completed records only
- **Unique Constraints**: Slot numbers must be unique
- **Foreign Keys**: Records properly linked to slots

## 📖 References

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [MudBlazor Components](https://mudblazor.com/)
- [MySQL Documentation](https://dev.mysql.com/doc/)

## 🤝 Support

For issues or questions:

1. Check the Troubleshooting section
2. Review TESTING_QUICK_START.md
3. Verify MySQL server is running and credentials are valid
4. Check application logs in Console.app

## 📝 License

This is an educational project for University of Moratuwa.

---

**Built with ❤️ using .NET MAUI Blazor Hybrid**
