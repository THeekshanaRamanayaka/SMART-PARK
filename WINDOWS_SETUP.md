# Windows Setup — SmartPark

This guide explains how to get SmartPark running on a Windows machine (developer workflow).

## Prerequisites

- Windows 10 (22H2+) or Windows 11
- Visual Studio 2022/2023 with **.NET MAUI** workload installed (recommended)
  - Ensure workloads: .NET Desktop, Mobile development with .NET, and .NET 10 SDK support
- Windows SDK / Windows App SDK (10.0.22621.0 or newer)
- Docker Desktop (for local MySQL)
- .NET 10 SDK (if using `dotnet` CLI)

## 1) Clone the repository

Open PowerShell or a terminal and clone the repo (if not already present):

```powershell
git clone <your-repo-url> "C:\dev\Smart-Park"
cd "C:\dev\Smart-Park"
```

## 2) Start MySQL (Docker)

From the repository root, run the Docker Compose stack that provides MySQL:

```powershell
docker compose up -d
```

The project expects MySQL to be reachable at `127.0.0.1:3307` by default (see repo `docker-compose.yml`).

## 3) Configure environment variables

Set the connection string and enable the local API while testing (PowerShell example):

```powershell
$env:SMARTPARK_MYSQL_CONNECTION = "server=127.0.0.1;port=3307;user=smartpark;password=smartpark;database=smartpark"
$env:SMARTPARK_ENABLE_LOCAL_API = "true"
```

You can alternatively add these to `launchSettings.json` (for Visual Studio) or a `.env` file if you use a runner that supports it.

## 4) Run in Visual Studio (recommended)

1. Open `Smart-Park.sln` in Visual Studio.
2. Ensure the `SmartPark` project is the Startup Project.
3. Select the target: **Windows Machine (Local)** or the `WinUI` target.
4. Press F5 (Debug) or Ctrl+F5 (Run without debugging).

Visual Studio will build necessary workloads and launch the app as a WinUI desktop application.

## 5) CLI build & run (alternative)

If you prefer the CLI, use explicit solution/project targets. The exact TFM for Windows may vary; try the Visual Studio approach if the CLI TFM fails.

```powershell
# restore
dotnet restore "Smart-Park.sln"

# build solution
dotnet build "Smart-Park.sln" -f net10.0-windows10.0.22621.0

# run the app project
dotnet run --project .\SmartPark\SmartPark.csproj -f net10.0-windows10.0.22621.0
```

If the `-f` framework fails, omit it and let the SDK pick the appropriate framework:

```powershell
dotnet run --project .\SmartPark\SmartPark.csproj
```

## 6) Local API & Postman

- When `SMARTPARK_ENABLE_LOCAL_API` is `true`, the app attempts to start a local API on `http://127.0.0.1:5099/` (used for cURL/Postman testing).
- On Windows this should be reachable by default. If you see permission or binding errors, set `SMARTPARK_ENABLE_LOCAL_API=false` to disable it.

Import `SmartPark.postman_collection.json` into Postman and set the environment variable `base_url` to `http://127.0.0.1:5099`.

Example cURL (entry):

```powershell
curl -X POST http://127.0.0.1:5099/api/records/entry -H "Content-Type: application/json" -d '{"vehicleNumber":"ABC-123","ownerName":"Driver"}'
```

## 7) Troubleshooting

- Build errors related to target framework: open the solution in Visual Studio and let it install the required MAUI workloads.
- If the app launches but the local API is not listening, check the runtime logs in Visual Studio Output window and toggle `SMARTPARK_ENABLE_LOCAL_API`.
- If MySQL connection fails, verify Docker container is running and that credentials/port match `SMARTPARK_MYSQL_CONNECTION`.

## 8) Publishing for Windows (optional)

To produce a distributable build you can publish the app (Visual Studio publish or CLI):

```powershell
dotnet publish .\SmartPark\SmartPark.csproj -c Release -f net10.0-windows10.0.22621.0 -o .\publish\win
```

Then distribute the contents of `publish\win` or package using MSIX from Visual Studio.

---

If you'd like, I can also add a sample `launchSettings.json` profile for Windows or adjust the project to include a Windows-specific TFM in the csproj. Do you want me to add that?
