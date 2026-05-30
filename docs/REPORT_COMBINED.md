# SmartPark — Combined Report & Technical Appendices

This file merges the primary project report with the technology justification and the architecture & design appendix for a single, consolidated reference.

---

## Final Project Report (Summary)

SmartPark is a desktop parking slot monitoring and management system built with .NET MAUI Blazor Hybrid and Entity Framework Core backed by MySQL. The system provides slot management, vehicle entry and exit recording, real-time active parking monitoring, searchable parking history, and reporting of completed sessions. This report documents the implemented system (design, implementation, testing, and evaluation) and maps features to the original assignment requirements.

### Key deliverables
- Project report and PDF
- Presentation slides (PPTX/PDF)
- Demo video (10–15 minutes)
- Source code and SQL scripts

### System Overview & Results
- All 10 functional requirements implemented: slot management, vehicle entry/exit, auto-assign slots, monitoring, search, reporting, CSV export.
- DB: MySQL with scripts in `Sql/` and a docker-compose file for local testing.

---

## Architecture & Design (Appendix)

See `ARCHITECTURE_AND_DESIGN.md` for full diagrams. In brief:
- Layered architecture: UI (Blazor), Services (business logic), Data (EF Core), Database (MySQL).
- Data flows: Vehicle Entry (autocomplete → create ParkingRecord → mark slot occupied), Vehicle Exit (calculate duration → free slot → persist).
- Key files: `MauiProgram.cs`, `Services/*`, `Data/SmartParkDbContext.cs`, `Components/Pages/*`.

---

## Technology Justification (Appendix)

High-level rationale for the choices made:
- **.NET MAUI Blazor Hybrid**: chosen for cross-platform desktop support, single C# codebase, and use on macOS (MacCatalyst) without needing Windows-only frameworks.
- **Entity Framework Core + Pomelo.MySql**: easier development with LINQ, strong productivity, and compatibility with MySQL.
- **MySQL**: open-source, scalable, and easy to run in Docker for reproducible local testing.
- **MudBlazor**: Material Design component library for professional UI and reduced development time.

---

## Testing & Validation
- E2E helper script: `run-e2e-ui-db-flow.sh` (seeds DB and verifies flows)
- SQL scripts: `Sql/init-mysql-schema.sql`, `Sql/e2e-ui-flow-seed.mysql.sql`, `Sql/e2e-ui-flow-verify.mysql.sql`
- Postman collection for local API tests (repo root)

---

## Appendices & How to Use
- To produce the PDF report: use Pandoc or VS Code Markdown PDF extension.
- Presentation and demo video guidelines included in `06-demo-video-script.md`.

---

For full details, see the original files in `docs/`:
- `Report.md`, `ARCHITECTURE_AND_DESIGN.md`, `TECHNOLOGY_JUSTIFICATION.md`.
