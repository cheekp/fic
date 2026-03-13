# F10 Programme Workspace Navigation Acceptance

## Acceptance Criteria

1. The active plan is `F10-programme-workspace-nav`, and `F09` has moved to completed plans.
2. Opening a merchant workspace without query parameters lands in the shop's programme operating path when a programme exists.
3. The workspace navigation makes `Programmes` a section within the shop workspace, not a peer root tab.
4. The `Programmes` section groups programmes into active, scheduled, and expired sections.
5. Switching between programmes preserves the nested programme section where practical.
6. Creating a programme makes the current delivery option explicit.
7. `Shop -> Overview` avoids join, stamping, and repeated programme dashboard content.
8. `Shop -> Programmes -> Operate` contains join QR, join link, stamping, issued passes, and programme activity.
9. `Shop -> Programmes -> Configure` contains programme rule, dates, and customer delivery settings, without customer join controls.
10. `Shop -> Programmes -> Insights` shows only selected-programme metrics.
11. `Shop -> Insights` shows only shop-wide metrics plus programme comparison.
12. The workspace avoids duplicate shop-settings links in the same view.
13. The selected programme header is compact and does not repeat a full summary dashboard.
14. The FIC utility chrome remains present but visually quieter than the merchant navigation.
15. `Programmes` uses a left programme rail and a single focused work pane rather than competing summary surfaces.
16. `Programmes -> Configure` keeps the delivery option explicit without a separate delivery-model explainer panel.
17. The F10 validator passes.

## Demo Walkthrough

1. Open a merchant workspace at `/portal/merchant/{id}` and confirm it lands in `Shop -> Programmes -> Operate`.
2. Confirm the shop navigation exposes `Overview`, `Programmes`, and `Insights`, with shop editing available as a secondary action.
3. Confirm the programme list is grouped by lifecycle status.
4. Switch from one programme to another while staying inside the same nested programme section.
5. Open `Shop -> Programmes -> Configure` and confirm join and stamping controls are absent while the loyalty card is presented as the current delivery output of the programme.
6. Confirm the programmes screen uses a selection rail on the left and one focused work pane on the right.
7. Confirm the configure view makes delivery management explicit, keeps wallet loyalty card as the current delivery option, and omits a separate delivery-model explainer panel.
8. Open `Shop -> Programmes -> Insights` and confirm only programme-level metrics are shown.
9. Open `Shop -> Insights` and confirm programme comparison appears there instead of programme operations.
10. Run the F10 validator and confirm build plus tests pass.
