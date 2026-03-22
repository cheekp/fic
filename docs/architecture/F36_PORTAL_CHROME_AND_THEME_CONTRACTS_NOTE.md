# F36 Portal Chrome And Theme Contracts Note

## Context

Next.js route parity exists, but portal-level structure is still page-local and inconsistent across signup/workspace slices. The Blazor variant has stronger shell cohesion (header/rail/surface language). We need a reusable portal chrome system that preserves FIC default identity while allowing future merchant theme variants.

## Decision

- Introduce reusable portal chrome primitives in Next.js:
  - `PortalShell` for frame composition
  - `PortalRail` for route-slice navigation
  - `PortalTopBar` with mobile burger interaction
  - `PortalRailDrawer` for mobile navigation drawer
  - `PortalSurface` primitives for hero/metric/card stacks
- Introduce frontend contracts for portal navigation and theme tokens.
- Keep FIC as default theme and support optional merchant overrides with safe fallbacks.
- Keep backend API contract as the source of truth for platform and merchant theme tokens; Next.js should consume compiled theme tokens rather than reconstructing visual language from only primary/accent values.

## Consequences

- Shared shell composition reduces UI entropy and duplicated nav/chrome logic.
- Mobile and desktop navigation behavior become consistent across routes.
- Future cross-app portal surfaces can reuse the same shell contract.
- Route slices become easier to migrate and validate under a single chrome system.
- Merchant workspace and join flows can stay visibly aligned because they read the same compiled brand token set.
