# RFC-005 Next.js Frontend And HTTP API Boundary

## Problem

The current Blazor UI is harder to evolve at startup speed because high-churn merchant workspace behavior is concentrated in large server components.

## Context

- Product direction still relies on .NET domain logic and Wallet capabilities.
- Founders want the frontend to move to React/Next.js.
- A hard cutover would pause feature delivery and increase risk.

## Options considered

1. Stay Blazor-only and continue incremental refactors.
2. Big-bang rewrite to Next.js and freeze Blazor changes.
3. Introduce `/api/v1`, run Next.js and Blazor in parallel, and migrate route-by-route.

## Decision

Adopt option 3.

- Add JSON endpoints for onboarding/session/workspace/programme operations.
- Build a Next.js app that consumes these endpoints.
- Keep Blazor live until each migrated route reaches acceptance parity.

## Consequences

- Temporary duplication of some frontend surface area.
- Faster frontend iteration once routes move to Next.js.
- Clear contract seam that improves future tenant/auth hardening.

## Open questions

- Final hosting shape for Next.js in production (standalone Node, static export + edge functions, or reverse-proxied app).
- Timing for retiring bUnit-first UX tests and replacing with frontend-native tests.
