# F48 Card Lifecycle + Bulk Operations Note

## Intent

Add explicit customer-card lifecycle controls (suspend/reactivate/archive) with bulk actions so operators can safely manage cards at scale.

## Decisions

- Introduce lifecycle states at the API/domain snapshot seam and surface them through existing card snapshot payloads.
- Add authenticated merchant endpoints for both single-card and bulk-card lifecycle actions.
- Keep redemption semantics intact: only reward-ready active cards can redeem.
- Extend customers lane with multi-select + bulk actions while preserving card detail drill-in.

## Outcome

- Better control over card hygiene and fraud/support workflows.
- Faster at-scale operations with reduced repetitive clicks.
- Contract-first extension aligned with portal/workspace architecture direction.
