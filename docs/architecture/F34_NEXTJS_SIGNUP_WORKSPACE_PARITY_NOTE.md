# F34 Next.js Signup And Workspace Parity Note

## Context

F33 established `/api/v1` and a minimal Next.js surface. The highest-friction merchant flow still lives in Blazor: signup plan and billing handoff, plus the daily programme operate/configure/customers loop.

## Decision

- Extend the Next.js migration surface to cover:
  - `/portal/signup/plan/{merchantId}`
  - `/portal/signup/billing/{merchantId}`
  - `/portal/merchant/{merchantId}` with `operate`, `configure`, and `customers` route slices.
- Reuse F33 API boundary, expanding client usage and tests for programme mutation and customer card actions.
- Keep Blazor routes active while parity stabilizes.

## Consequences

- Merchant-first flow can be iterated in React/Next.js without blocking on Blazor component edits.
- API boundary becomes the primary interaction contract for programme operations.
- Additional frontend-native tests can be layered later without breaking current .NET proof loops.
