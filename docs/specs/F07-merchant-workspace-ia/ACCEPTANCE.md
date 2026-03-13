# F07 Merchant Workspace IA Acceptance

## Acceptance Criteria

1. The home page behaves as a FIC landing surface, not as a multi-merchant demo console.
2. Merchant signup uses workspace-oriented language and lands the user directly in the merchant workspace.
3. The merchant workspace separates `Brand`, `Loyalty Card`, and `Customers`.
4. Brand is the default workspace section.
5. Merchants can update workspace branding from inside the workspace.
6. Merchants can update the loyalty card template from inside the workspace.
7. Issued customer cards are presented as customer instances, distinct from the card template.
8. The local multi-workspace list exists only on a dev-oriented route.
9. The solution builds successfully with the refactored flow.

## Demo Walkthrough

1. Open `/`.
2. Start merchant signup.
3. Create the merchant workspace.
4. Confirm the app lands on `/portal/merchant/{id}?tab=brand`.
5. Update brand details from the workspace.
6. Switch to `Loyalty Card` and update the card template.
7. Switch to `Customers` and confirm the join QR, scan tools, and issued cards live there.
8. Open `/dev/workspaces` and confirm the old multi-workspace behavior has been isolated to a dev route.
