# Fic.Platform.Frontend

Next.js migration surface for the FIC merchant frontend.

## Stack

- Next.js + TypeScript
- Tailwind CSS
- shadcn-style UI primitives
- Radix interaction primitives
- PWA manifest and service-worker shell

## Local Run

1. Start API host:
   - `dotnet run --project src/Fic.Platform.Web`
2. Install frontend deps:
   - `cd src/Fic.Platform.Frontend && npm install`
3. Start Next.js:
   - `npm run dev`

Optional env var:

- `NEXT_PUBLIC_FIC_API_BASE_URL` (default: `http://localhost:5276`)

## Current Route Slices

- `/` migration landing page
- `/portal/signup` merchant creation
- `/portal/signup/plan/[merchantId]` plan selection
- `/portal/signup/billing/[merchantId]` owner credential setup
- `/portal/merchant/[merchantId]` workspace parity slices for `operate`, `configure`, and `customers`
