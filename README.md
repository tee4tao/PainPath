# PainPath

AR-powered pain communication platform for physiotherapy consultations.

## Projects

| Folder | Description | Tech |
|---|---|---|
| `/unity` | Patient-facing AR pain mapping app | Unity, Meta Quest 3 |
| `/physio-portal` | Clinician review dashboard | Next.js, Tailwind, Firebase |

## How it works
1. Patient paints pain zones on AR body model in the Quest 3 app
2. Unity POSTs session JSON to `/api/sessions`
3. Gemini analyses the pain pattern and generates an exercise plan
4. Physio reviews and approves via the portal
5. Approved plan syncs back to the Quest 3 headset

## Setup
See `README.md` in each subfolder for project-specific instructions.