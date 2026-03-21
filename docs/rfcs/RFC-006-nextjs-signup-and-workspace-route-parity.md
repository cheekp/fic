# RFC-006 Next.js Signup And Workspace Route Parity

## Problem

Merchant onboarding and day-to-day programme operations are still coupled to Blazor route surfaces, slowing React/Next migration value.

## Options considered

1. Keep only minimal Next surface and delay route migration.
2. Migrate signup plan/billing and workspace operate/configure/customers immediately via `/api/v1`.
3. Attempt complete route parity in one hard cut.

## Decision

Adopt option 2.

- Prioritize the highest-frequency merchant routes.
- Keep migration incremental with API and test expansion.
- Avoid hard cutover risk while increasing real-world usage of Next surfaces.

## Consequences

- Two frontend implementations coexist temporarily.
- Faster confidence-building with production-like route slices.
- Clear handoff path for retiring Blazor routes once acceptance parity is proven.
