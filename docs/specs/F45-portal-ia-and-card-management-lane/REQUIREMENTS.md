# F45 Requirements

## Functional

1. Public routes expose one consistent top header with:
   - left burger trigger for site hierarchy/navigation links,
   - right-side log in and sign up actions.
2. Merchant workspace route avoids duplicate/competing side navigation panes in the main operating composition.
3. Customers slice renders a table-oriented card management lane with:
   - card preview surface,
   - key card metadata (code, status, progress, date window, last update),
   - row-level actions for operational tasks.
4. Customers lane includes quick workflow actions for card lifecycle operation (for example demo join and redeem where supported).
5. Existing signup/roadmap/workspace route seams continue to operate on existing API contracts.

## Non-Functional

1. Desktop layout remains readable at common widths (for example 1366+), without major squeeze artifacts.
2. Mobile layout remains functional with horizontal table overflow handled intentionally.
3. No regressions in existing frontend build and QA scripts.
