# F11 Programme Workspace Polish Acceptance

## Acceptance Criteria

1. The active plan is `F11-programme-workspace-polish`, and `F10` has moved to completed plans.
2. The merchant workspace primary navigation remains `Overview`, `Programmes`, and `Insights`.
3. Shop settings opens as an in-context surface instead of only as a standalone workspace page.
4. Existing links that previously targeted the edit page still surface shop settings successfully.
5. The programmes screen uses a tighter left rail that focuses on grouped programme selection and creation.
6. The programmes rail avoids large explainer copy once the grouped programme list is visible.
7. Creating a programme still makes the current delivery output explicit.
8. The selected programme header stays compact and shows only essential programme context.
9. `Programmes -> Operate` remains the default working view and still exposes join, stamping, issued passes, and activity.
10. `Programmes -> Configure` keeps the programme form, delivery output, and preview in one calmer work surface.
11. The workspace avoids redundant explanatory panels in the programme configuration area.
12. The F11 validator passes.

## Demo Walkthrough

1. Open a merchant workspace and confirm it still defaults into `Shop -> Programmes -> Operate`.
2. Open shop settings from the merchant workspace and confirm it appears as a lighter in-context surface.
3. Navigate to the old edit route and confirm the same shop settings surface still appears.
4. Open `Programmes` and confirm the left side behaves like a programme rail rather than a second content page.
5. Start creating a programme and confirm `Wallet loyalty card` remains explicit as the current delivery output.
6. Open `Programmes -> Configure` and confirm the preview remains present without a stack of redundant explainer panels.
7. Run the F11 validator and confirm build plus tests pass.
