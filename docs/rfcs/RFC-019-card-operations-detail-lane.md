# RFC-019 Card Operations Detail Lane

## Problem

Customers route supports table-level scanning but lacks enough per-card context for high-volume operation, leading to more guesswork before action.

## Decision

Enhance the customers management lane with:

- status-based filtering,
- stronger operational sort order,
- card detail drill-in with preview + metadata + recent activity,
- mirrored row/detail actions for critical card operations.

## Scope

- Next.js customers section in merchant workspace.
- No backend/API contract changes.
- Keep existing roadmap/onboarding behavior untouched.

## Consequences

- Better operator efficiency at scale.
- Reduced mistakes in redemption and support workflows.
- Stronger UX parity with intended “premium operations” direction.
