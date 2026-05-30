# SmartPark — VIVA Combined Guide

This file aggregates all VIVA preparation material into a single reference for quick review and printing.

---

## Master Index (condensed)

See the following sections for the consolidated preparation material: Quick Reference, Preparation Guide, Architecture, and Key Q&A.

### Priority reading order

- Quick Reference (print & keep during VIVA)
- Code Navigation map (open in editor during VIVA)
- Preparation Guide (detailed study)

---

## Quick Reference Card

> Elevator pitch (30s)

SmartPark is a desktop parking management system for small organizations (schools, offices, small lots). Built with .NET MAUI Blazor Hybrid + MySQL. It handles vehicle entry/exit, real-time slot monitoring, searchable history, and reporting. Offline-first, no internet required.

Key files you must know:

- `MauiProgram.cs` — app initialization & DI
- `Services/ParkingRecordService.cs` — entry/exit logic
- `Components/Pages/VehicleEntry.razor` — entry UI + autocomplete
- `Data/SmartParkDbContext.cs` — EF Core context

10 Functional Requirements (all implemented): Manage slots, register vehicle, assign slots, monitor availability, record entry, record exit, auto-update slot status, view records, search records, generate reports.

---

## VIVA Preparation Guide (condensed)

Overview, architecture, tech stack, and preparation checklist.

- Project: SmartPark — desktop parking system using .NET MAUI Blazor Hybrid
- Core tech: .NET 10, MAUI Blazor, EF Core (Pomelo MySQL), MudBlazor
- Project structure: Models, Data (DbContext), Services, Components (Blazor pages)

Prep checklist:

- Read Quick Reference and Code Navigation map (30 min)
- Read Preparation Guide (45 min)
- Run the app locally and walk through demo flow (30 min)
- Practice 2-minute demo and 60-second elevator pitch
- Prepare 3 code excerpts to explain

Run the app (quick):

```bash
docker compose up -d
dotnet restore "Smart-Park.sln"
dotnet run --project ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst
```

---

## Architecture & Design (high-level)

Layered architecture:

- UI (Blazor components under `Components/Pages`)
- Services (business logic: `ParkingSlotService`, `ParkingRecordService`)
- Data (EF Core `SmartParkDbContext`)
- Database (MySQL; scripts in `Sql/`)

Key flows: Vehicle entry (autocomplete → select/auto-assign slot → create record → mark slot occupied) and Vehicle exit (calculate duration → free slot → move to completed records).

---

## Top 10 VIVA Questions & Short Answers

1. Why MAUI Blazor Hybrid? — Cross-platform (macOS + Windows), single C# codebase, uses web-like components via Blazor with native performance.
2. Walk through vehicle entry flow — (short numbered flow; be ready to open `VehicleEntry.razor`).
3. How prevent overbooking? — `IsOccupied` flag + validation at entry; fallback to auto-assign.
4. What's not implemented? — Authentication and formal EF migrations; listed as future work.
5. How tested? — Manual E2E flow, Postman, SQL verification scripts.
6. What files to open if asked? — `Services/ParkingRecordService.cs`, `VehicleEntry.razor`, `SmartParkDbContext.cs`.
7. Demo steps (2-minute version) — Dashboard, Vehicle Entry, Active Parking, Exit, Search + Report export.
8. How to run local API checks? — `curl http://127.0.0.1:5099/health`.
9. Where to find DB scripts? — `Sql/init-mysql-schema.sql` and seed scripts.
10. Elevator pitch — memorize the 60-second pitch in the master index.

---

## Quick Links

- `DOCS_INDEX.md` — full docs index
- `CODE_NAVIGATION_MAP.md` — mapping features → files
- `Report.md` — final project report

Good luck — practice the demo and keep the quick reference printed nearby.
