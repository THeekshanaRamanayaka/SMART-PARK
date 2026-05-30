# SmartPark — Demo Video Combined Script

This file contains the full demo video script, shot list, recording guidelines, and commands in one place for recording the 10–15 minute demo video.

---

## Demo Video Script & Presentation Guide (10–15 minutes)

Purpose: Comprehensive presentation covering objectives, architecture, design, and implementation, followed by a live demonstration of core flows: DB startup, app launch, slot management, vehicle entry (autocomplete + manual slot selection), exit recording, and CSV report generation.

### SECTION 1: PRESENTATION SLIDES (0:00–3:00)
0:00–0:30 — Title & Introduction
- On-screen: Title slide with project name and student details
- Narration: short project pitch

0:30–1:15 — Problem Statement & System Context
- Slide: Problem domain and target users

1:15–2:00 — Architecture & Design
- Slide: System architecture diagram

2:00–3:00 — Key Features & Implementation
- Slide: Implemented features checklist

### SECTION 2: LIVE DEMONSTRATION (3:00–12:00)
3:00–3:45 — Environment Setup (docker compose, restore, run app)
3:45–5:00 — Manage Slots Page (create/edit slots)
5:00–6:30 — Vehicle Entry with Autocomplete (known and new vehicle flows)
6:30–7:45 — Active Parking View (show active session)
7:45–9:00 — Record Vehicle Exit (calculate duration, free slot)
9:00–10:30 — Reports & CSV Export (generate, open CSV)
10:30–11:15 — Optional: Local API & Testing (curl examples)
11:15–12:00 — Conclusions & Next Steps

### SECTION 3: Q&A (12:00–15:00)
- Allocate remaining time for questions; be ready to show code.

---

## Recording Guidelines
- Resolution: 1920×1080, 30 fps, audio clear
- Test app and DB startup; pre-seed DB as needed
- Use terminal with large font; keep desktop clean

---

## Commands Reference
```bash
# Start MySQL
docker compose up -d

# Restore and run
dotnet restore "Smart-Park.sln"
dotnet run --project ./SmartPark/SmartPark.csproj -f net10.0-maccatalyst

# Local API checks
curl http://127.0.0.1:5099/health
curl http://127.0.0.1:5099/api/slots
```

---

## Deliverables Checklist for Video
- [ ] Presentation file renamed and uploaded
- [ ] Demo video recorded (10–15 mins) and checked for audio/video quality
- [ ] Sample data seeded and verified
