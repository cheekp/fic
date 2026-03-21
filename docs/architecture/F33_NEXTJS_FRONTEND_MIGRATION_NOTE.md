# F33 Next.js Frontend Migration Note

## Context

The merchant-facing UI has accumulated high-change product behavior inside large Blazor components. Iteration speed has slowed because UI behavior and rendering concerns are tightly coupled to component internals and bUnit snapshots.

Founders have requested a React/Next.js direction for frontend delivery while retaining current .NET domain and Wallet seams.

## Decision

- Introduce a versioned HTTP API boundary at `/api/v1` in `Fic.Platform.Web` for merchant onboarding, session, workspace, and programme operations.
- Create a new Next.js app at `src/Fic.Platform.Frontend` that consumes `/api/v1`.
- Keep Blazor routes active during migration to avoid delivery freeze.
- Migrate route-by-route from Blazor to Next.js while preserving existing domain services (`DemoPlatformState`, wallet services, brand services).

## Consequences

- Frontend can iterate independently from Blazor component structure.
- Product behavior moves toward API contracts instead of server component coupling.
- Existing Blazor-based validators and tests will be retired incrementally as Next.js parity lands.
- The .NET backend remains the business/domain system of record; this slice is a frontend and transport seam migration, not a backend rewrite.
