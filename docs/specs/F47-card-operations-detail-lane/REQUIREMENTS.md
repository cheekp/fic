# F47 Requirements

## Functional

1. Customers lane supports status filtering for card subsets.
2. Customers lane applies stable, useful ordering for at-scale scanning (most recently updated first).
3. Each card row exposes a detail action that opens a card detail dialog.
4. Card detail dialog shows:
   - branded card preview,
   - card status/progress,
   - core metadata (code, reward label, date window, updated timestamp),
   - recent timeline activity relevant to the card.
5. Card actions remain available and consistent from both row and detail views.

## Non-Functional

1. No API payload changes required.
2. Existing build/tests/QA scripts remain green.
3. Mobile and desktop remain usable without horizontal overflow regressions beyond intentional table scroll.
