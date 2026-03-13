# F12 Company Brand Surfaces Acceptance

## Acceptance Criteria

1. The active plan is `F12-company-brand-surfaces`, and `F11` has moved to completed plans.
2. The home page uses North Star-inspired tone, palette, and positioning while preserving a clear FIC signup CTA.
3. The home page does not depend on the supplied company logo image.
4. Login and forgot-password feel like branded company support/account surfaces rather than bare MVP forms.
5. Merchant utility chrome reads as company support and exposes destinations for consultancy, billing, and account.
6. Lightweight routed stubs exist for consultancy, billing, and account support surfaces.
7. The loyalty agent wording changes by context and remains discreet.
8. Merchant-owned shop/programme surfaces remain merchant-themed rather than being re-skinned into the company layer.
9. The F12 validator passes.

## Demo Walkthrough

1. Open `/` and confirm the page feels like a company-backed product entry point, not just like a product stub.
2. Confirm the home page still clearly leads to `/portal/signup`.
3. Open `/account/login` and `/account/forgot-password` and confirm they share the company brand layer.
4. Open the merchant workspace and confirm the top utility menu reads as company support rather than primary navigation.
5. Open the consultancy, billing, and account support routes from the utility menu and confirm they resolve to branded stub surfaces.
6. Confirm the merchant workspace content still follows merchant theming rather than the company palette.
7. Run the F12 validator and confirm build plus tests pass.
