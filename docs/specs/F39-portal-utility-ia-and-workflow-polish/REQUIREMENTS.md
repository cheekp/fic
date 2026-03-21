# F39 Requirements

## Functional

1. Portal header provides utility links for blogs, training, consultancy, account, billing, and logout.
2. Utility routes resolve as first-party pages in Next.js for blogs/training/consultancy/account/billing.
3. Signup plan and billing screens avoid duplicate informational side panes.
4. Onboarding flow remains intact: signup -> plan -> billing -> workspace.
5. Signup account step captures essential account fields (shop name, owner name, owner email); shop type selection moves to shop setup in workspace onboarding.
6. Workspace onboarding shop setup supports merchant logo upload, aligned with previous Blazor capability.
7. Onboarding roadmap includes a dedicated owner access stage before billing.
8. Billing route separates payment confirmation from owner credential entry and enforces owner-access confirmation before billing confirmation.
9. Workspace navigation avoids heavy side rail layout and uses a compact inline section switcher for Operate/Configure/Customers.
10. Workspace rail visuals (where rendered) avoid redundant completion tick marks; roadmap remains the primary setup progress surface.
11. Roadmap current stage/completion stays synchronized with latest onboarding state after save/upload/create actions.
12. Brand logo upload applies immediately to Next.js surfaces without stale-cache artifacts.
13. Merchant workspace flow does not expose a desktop rail-toggle chrome control that changes primary content width.
14. Merchant shop setup opens as a centered modal, not a right-edge full-height drawer.

## Non-Functional

1. Visual hierarchy prioritizes one primary action per onboarding screen.
2. Utility surfaces follow the existing brand language and spacing tokens.
