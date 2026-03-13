# F09 Merchant Workspace Polish Acceptance

## Acceptance Criteria

1. The workspace uses a horizontal onboarding roadmap that can be collapsed and dismissed.
2. The shop tab keeps editing focused on shop details and branding rather than mixing in card operations.
3. The loyalty cards tab makes multi-card management explicit.
4. The customers tab makes it clear that join and stamping belong to the selected loyalty card.
5. The insights tab separates shop-level and selected-card metrics in the rendered UI.
6. FIC utility chrome remains visually distinct from merchant-owned workspace content.
7. The existing workspace tests still pass, and any changed UI behavior has matching test updates.
8. The F09 validator passes.

## Demo Walkthrough

1. Open a merchant workspace and land on the shop tab.
2. Confirm the tab reads as shop setup and ownership, not as a generic dashboard.
3. Switch to `Loyalty Cards` and confirm multiple programmes are clearly managed there.
4. Switch to `Customers` and confirm the selected card drives the join and stamping surfaces.
5. Switch to `Insights` and confirm shop-level versus selected-card metrics are distinct.
6. Run the F09 validator and confirm build plus tests pass.
