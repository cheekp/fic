# F48 Requirements

## Functional

1. Card lifecycle supports `suspend`, `reactivate`, and `archive` transitions.
2. Lifecycle state is reflected in `WalletCardSnapshot` status and labels.
3. Single-card lifecycle API endpoint exists and enforces merchant ownership.
4. Bulk lifecycle API endpoint exists and applies a transition to multiple cards in one request.
5. Customers lane supports multi-select and bulk lifecycle actions.
6. Status filters include suspended/archived states.

## Non-Functional

1. Existing redeem and award flows remain functional for active cards.
2. Unauthorized/foreign-merchant lifecycle mutation attempts are rejected.
3. Slice includes validator coverage and API tests.
