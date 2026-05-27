# SmartPark Testing Quick Start

This project is a desktop .NET MAUI Blazor Hybrid app using MySQL.

## 1) Configure MySQL

Start the Docker container first:

```bash
docker compose up -d
```

```bash
export SMARTPARK_MYSQL_CONNECTION="server=127.0.0.1;port=3307;database=smartpark;user=smartpark;password=smartpark;"
export SMARTPARK_MYSQL_HOST=127.0.0.1
export SMARTPARK_MYSQL_PORT=3307
export SMARTPARK_MYSQL_DB=smartpark
export SMARTPARK_MYSQL_USER=smartpark
export SMARTPARK_MYSQL_PASSWORD=smartpark

# Optional: disable the local HTTP API if you only want the desktop UI
export SMARTPARK_ENABLE_LOCAL_API=true
```

## 2) Build and run desktop app

```bash
cd SmartPark
dotnet restore
dotnet build -f net10.0-maccatalyst
dotnet run -f net10.0-maccatalyst
```

## 3) Seed and verify E2E test data

From repo root:

```bash
./run-e2e-ui-db-flow.sh
```

## 4) Desktop UI end-to-end check

1. Open Vehicle Entry and register:

- Vehicle Number: E2E-UI-ENTRY
- Owner Name: UI Test Driver

2. Open Active Parking and click Record Exit for E2E-UI-ENTRY.
2. Re-run:

```bash
./run-e2e-ui-db-flow.sh
```

## 5) Direct DB verification queries

```bash
MYSQL_PWD="${SMARTPARK_MYSQL_PASSWORD:-smartpark}" mysql -t -h "${SMARTPARK_MYSQL_HOST:-127.0.0.1}" -P "${SMARTPARK_MYSQL_PORT:-3306}" -u "${SMARTPARK_MYSQL_USER:-smartpark}" "${SMARTPARK_MYSQL_DB:-smartpark}" -e "SELECT VehicleNumber, OwnerName, EntryTime, ExitTime, IsCompleted FROM ParkingRecords WHERE VehicleNumber='E2E-UI-ENTRY' ORDER BY Id DESC LIMIT 1;"
```

```bash
MYSQL_PWD="${SMARTPARK_MYSQL_PASSWORD:-smartpark}" mysql -t -h "${SMARTPARK_MYSQL_HOST:-127.0.0.1}" -P "${SMARTPARK_MYSQL_PORT:-3306}" -u "${SMARTPARK_MYSQL_USER:-smartpark}" "${SMARTPARK_MYSQL_DB:-smartpark}" -e "SELECT s.SlotNumber, s.IsOccupied FROM ParkingSlots s JOIN ParkingRecords r ON r.ParkingSlotId=s.Id WHERE r.VehicleNumber='E2E-UI-ENTRY' ORDER BY r.Id DESC LIMIT 1;"
```

## 6) Local API tests

```bash
curl http://127.0.0.1:5099/health
```

```bash
curl http://127.0.0.1:5099/api/records/active
```

```bash
curl -X POST http://127.0.0.1:5099/api/records/entry \
 -H "Content-Type: application/json" \
 -d '{"vehicleNumber":"E2E-UI-ENTRY","ownerName":"UI Test Driver"}'
```

```bash
curl -X POST http://127.0.0.1:5099/api/records/1/exit
```

The terminal also shows runtime logs for schema initialization, slot allocation, record creation, record exit, and API requests.
