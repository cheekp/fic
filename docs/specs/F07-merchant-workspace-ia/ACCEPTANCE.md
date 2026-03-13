# F07 Merchant Workspace IA Acceptance

## Acceptance Criteria

1. The home page behaves as a clean FIC landing surface with a primary sign-up call to action, supporting quote/image treatment, and standard account entry links.
2. Merchant signup uses shop-owner language, stays thin, captures real shop/account basics, and routes into a mock billing step before the merchant workspace.
3. The merchant workspace separates `Brand`, `Loyalty Card`, `Customers`, and `Insights`.
4. Brand is the default workspace section.
5. Merchants can update workspace branding from inside the workspace.
6. Merchants can update the loyalty card template from inside the workspace.
7. Loyalty cards expose begin and expiry dates, and those dates govern join and stamping availability.
8. Issued customer cards are presented as customer instances, distinct from the card template.
9. The local multi-workspace list exists only on a dev-oriented route.
10. The live merchant workspace no longer uses the persistent `FIC` left rail.
11. The merchant workspace exposes only lightweight platform utility actions for `Training & Consultancy`, `Billing`, `Account`, and `Log Out`.
12. Merchant card and customer management remain inside the merchant workspace itself.
13. A discreet floating agent launcher appears across the site with route-appropriate context.
14. `Speak with Loyalty Agent` appears as a floating support launcher rather than a primary workspace tab or dominant button.
15. The solution builds successfully with the refactored flow.
16. Automated tests cover the merchant state transitions for multi-programme behavior and loyalty-card date windows.

## Demo Walkthrough

1. Open `/`.
2. Confirm the home page shows `Sign Up Now`, `Log In`, and `Forgot password`.
3. Start merchant signup.
4. Confirm signup asks only for the shop basics rather than card-template fields.
5. Create the merchant workspace.
6. Confirm the mock billing step appears before the workspace.
7. Confirm the app lands on `/portal/merchant/{id}?tab=brand`.
8. Update brand details from the workspace.
9. Switch to `Loyalty Card` and update the card template, including begin and expiry dates.
10. Confirm customer join and till-side stamping are only available while the selected card is inside its active date window.
11. Switch to `Customers` and confirm the join QR, scan tools, and issued cards live there.
12. Switch to `Insights` and confirm the navigation model supports analytics/training expansion.
13. Confirm the live merchant workspace opens without the persistent `FIC` left rail.
14. Confirm `Training & Consultancy`, `Billing`, `Account`, and `Log Out` appear only as lightweight utility actions.
15. Confirm the floating agent launcher changes context appropriately across home, signup, billing, and merchant workspace routes.
16. Confirm `Speak with Loyalty Agent` appears as a floating support launcher rather than a main navigation item.
17. Open `/dev/workspaces` and confirm the old multi-workspace behavior has been isolated to a dev route.
18. Run the focused xUnit suite and confirm the multi-programme/date-window rules pass automatically.
