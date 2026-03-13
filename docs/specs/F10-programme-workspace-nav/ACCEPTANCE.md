# F10 Programme Workspace Navigation Acceptance

## Acceptance Criteria

1. The active plan is `F10-programme-workspace-nav`, and `F09` has moved to completed plans.
2. Opening a merchant workspace without query parameters lands in the programme operating path when a programme exists.
3. The `Programmes` scope groups programmes into active, scheduled, and expired sections.
4. Switching between programmes preserves the nested programme section where practical.
5. `Programmes -> Operate` contains join QR, join link, stamping, issued passes, and programme activity.
6. `Programmes -> Configure` contains programme rule, dates, wallet copy, and wallet preview, without customer join controls.
7. `Programmes -> Insights` shows only selected-programme metrics.
8. `Shop -> Insights` shows only shop-wide metrics plus programme comparison.
9. The FIC utility chrome remains present but visually quieter than the merchant navigation.
10. The F10 validator passes.

## Demo Walkthrough

1. Open a merchant workspace at `/portal/merchant/{id}` and confirm it lands in `Programmes -> Operate`.
2. Confirm the programme list is grouped by lifecycle status.
3. Switch from one programme to another while staying inside the same nested programme section.
4. Open `Programmes -> Configure` and confirm join and stamping controls are absent.
5. Open `Programmes -> Insights` and confirm only programme-level metrics are shown.
6. Open `Shop -> Insights` and confirm programme comparison appears there instead of programme operations.
7. Run the F10 validator and confirm build plus tests pass.
