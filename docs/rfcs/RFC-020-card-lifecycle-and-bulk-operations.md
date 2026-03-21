# RFC-020 Card Lifecycle And Bulk Operations

## Problem

Card operations currently focus on redeem/copy but do not support explicit operator lifecycle control for problematic or retired cards, especially in high-volume scenarios.

## Decision

Implement a contract-backed lifecycle model with:

- `suspend` (temporarily disable card operations),
- `reactivate` (return to normal status evaluation),
- `archive` (remove from day-to-day active operation lanes),
- bulk lifecycle action endpoints for selected cards.

## Scope

- `Fic.Contracts` status expansion for card lifecycle labels.
- `DemoPlatformState` lifecycle state storage and transition behavior.
- `/api/v1` lifecycle endpoints (single + bulk).
- Next.js customers lane bulk-select + action controls.

## Consequences

- Stronger operational controls for support/fraud workflows.
- Better UX for at-scale card management without introducing new surface sprawl.
