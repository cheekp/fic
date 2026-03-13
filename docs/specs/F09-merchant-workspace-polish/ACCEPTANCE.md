# F09 Merchant Workspace Polish Acceptance

## Acceptance Criteria

1. The workspace uses a horizontal onboarding roadmap that can be collapsed and dismissed.
2. The workspace uses `Shop` and `Programmes` as the primary scopes.
3. The `Shop` scope contains `Overview`, `Edit Shop`, and `Insights`.
4. The `Programmes` scope contains `Operate`, `Configure`, and `Insights`.
5. Customer join and till-side stamping appear only inside programme `Operate`.
6. Programme configuration, dates, and wallet-copy editing appear only inside programme `Configure`.
7. Shop-level insights and selected-programme insights are rendered in separate scopes.
8. FIC utility chrome remains visually distinct from merchant-owned workspace content.
9. The existing workspace tests still pass, and any changed UI behavior has matching test updates.
10. The F09 validator passes.

## Demo Walkthrough

1. Open a merchant workspace and confirm the top-level scopes are `Shop` and `Programmes`.
2. Open `Shop -> Overview` and confirm the roadmap is secondary to the main workspace.
3. Open `Shop -> Edit Shop` and confirm only shop profile and branding controls appear there.
4. Open `Programmes -> Operate` and confirm the selected programme drives join QR, stamping, and issued passes.
5. Open `Programmes -> Configure` and confirm the selected programme rule, validity dates, and wallet copy can be edited there.
6. Open `Shop -> Insights` and `Programmes -> Insights` and confirm the metrics are separated by scope.
7. Run the F09 validator and confirm build plus tests pass.
