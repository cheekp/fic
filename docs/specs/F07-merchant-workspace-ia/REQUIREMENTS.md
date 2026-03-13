# F07 Merchant Workspace IA Requirements

## Objective

Refactor the internal demo flow so it feels like a merchant product rather than a multi-workspace demo console.

## Scope

- Keep `FIC` as the product-level shell.
- Make merchant onboarding lead directly into a merchant-owned workspace.
- Make the merchant workspace the place where merchants manage:
  - brand
  - loyalty card template
  - issued customer cards
- Remove the “demo workspaces” concept from the main merchant-facing home experience.

## Functional Requirements

### Merchant Journey

- `/` should behave as a clean FIC landing page with a single primary path into signup.
- The home page should present:
  - a primary `Sign Up Now` call to action
  - the business-plan quote about value, recognition, experience, and connection
  - a supporting hero image
  - standard account entry links for `Log In` and `Forgot password`
- Merchant signup should create the merchant workspace behind the scenes, but the public language should be shop-owner oriented rather than internal platform jargon.
- Merchant signup should stay intentionally thin and collect only shop/account basics.
- Merchant signup should capture at least:
  - coffee shop name
  - town or city
  - postcode
  - owner email
- Loyalty programme or card-template fields must not be configured during merchant signup.
- Merchant signup may create a starter default programme behind the scenes for demo speed, but richer brand/card editing must happen in the workspace itself.
- After shop signup, the merchant should pass through a mock billing step before landing in their workspace.

### Workspace Information Architecture

- The merchant workspace must clearly separate:
  - `Brand`
  - `Loyalty Card`
  - `Customers`
  - `Insights`
- Brand should be the default workspace section.
- The workspace should make it obvious that:
  - brand is the merchant-level source of truth
  - the loyalty card template inherits from that brand by default
  - issued customer cards are separate runtime instances of that template
- The live merchant workspace should not feel like a multi-merchant admin console.
- Merchant card and customer operations must not be represented as top-level `FIC` navigation.

### Brand Management

- Merchants must be able to update business name, primary colour, accent colour, and logo from inside the workspace.
- Merchant logo upload should continue to support PNG and palette suggestion.
- Updating workspace branding must refresh the merchant workspace snapshot.

### Loyalty Card Template Management

- Merchants must be able to update:
  - reward item label
  - reward threshold
  - reward copy
  - begin date
  - expiry date
- The wallet card preview should reflect the current merchant brand plus the current loyalty card template.
- Customer join and till-side stamping must respect the configured begin and expiry dates.
- Existing demo customer cards should be adjusted to the updated template consistently enough for internal demo purposes.

### Customer Card Operations

- Join QR and customer scan flow should live under the `Customers` section.
- Issued customer cards should be described explicitly as issued cards, not as the card template itself.
- Reward redemption and visit award flow must remain available from the merchant workspace.

### Demo-Only Workspace Index

- Browsing multiple local demo workspaces should move to a separate dev-oriented route.
- That route must not dominate the default merchant-facing home experience.

### Platform Chrome

- `FIC` should remain the acquisition and product-level shell for home and signup.
- The live merchant workspace should switch to a lightweight merchant-owned frame rather than a persistent left rail.
- Platform-level concerns such as `Training & Consultancy`, `Billing`, `Account`, and `Log Out` should be available as subtle workspace utility actions, not as dominant navigation.
- A discreet floating agent launcher should be available across the site, with context-specific behavior:
  - home and account entry routes: sales/help context
  - signup: setup help context
  - billing: billing help context
  - merchant workspace: loyalty/help context
- `Speak with Loyalty Agent` should behave like a floating support/contact surface rather than a primary page tab or dominant button.

## Non-Goals

- Full auth and tenant isolation
- Production navigation design
- Persistent per-user merchant session selection
- Card-level design overrides
