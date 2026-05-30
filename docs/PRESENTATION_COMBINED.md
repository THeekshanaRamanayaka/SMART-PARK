# SmartPark — Presentation Combined Notes

This file collects presentation slide outlines, timing guidance, and naming conventions into a single reference for preparing the final slide deck.

---

## Slide Outline (10–15 minutes)

1. Title & Introduction (30s)
   - Project name, student details (Surname_Initials_IndexNo)
2. Problem Statement & System Context (45s)
   - Gap in existing solutions; target users
3. Architecture & Design (45s)
   - Layered architecture diagram (Blazor → Services → EF Core → MySQL)
4. Key Features & Implementation (60s)
   - Autocomplete, auto-assign slots, real-time dashboard, CSV export
5. Live Demo Plan (30s) — explain what you'll demo
6. Results & Testing (45s)
   - E2E script, verification queries, seed data
7. Gap Analysis & Future Work (45s)
   - Authentication, migrations, testing
8. Conclusions & Q&A (Remaining time)

---

## Slide Tips
- Keep slides visual: diagrams and screenshots
- Avoid dense code on slides; show code live if asked
- Include one slide with demo steps and commands (copyable during recording)

---

## Presentation File Naming
- Required format for submission: `Surname_Initials_IndexNo_Presentation.pptx` (or `.pdf`)

---

## Quick Commands to Display (copy into a terminal slide)
```bash
docker compose up -d
dotnet restore "Smart-Park.sln"
dotnet run --project ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst
```

---

## Helpful Assets
- `SmartPark_Presentation.pptx` — existing slide deck (rename before submission)
- Screenshots in `Resources/Splash` and `Resources/Images` for slide visuals
