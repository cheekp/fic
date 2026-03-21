# F47 Card Operations Detail Lane Note

## Intent

Move customer-card management from list-level actions to an operator-grade workflow with card-level inspection and action context.

## Decisions

- Extend the existing table-first customers lane with operational filtering and card-level drill-in.
- Add a card detail dialog that combines visual preview, key metadata, and recent activity events.
- Keep primary lifecycle actions (copy code, redeem) accessible from both table row and detail view.
- Reuse existing workspace snapshot/timeline payloads; no API contract expansion in this slice.

## Outcome

- Faster triage when card counts grow.
- Better confidence in redemption actions through visible status/progress context.
- Cleaner “manage at scale” story without reintroducing heavy dashboard noise.
