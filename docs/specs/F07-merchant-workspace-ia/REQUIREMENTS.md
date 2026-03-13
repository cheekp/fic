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

- `/` should behave as the FIC landing and launch surface.
- Merchant signup should create the merchant workspace behind the scenes, but the public language should be shop-owner oriented rather than internal platform jargon.
- Merchant signup should stay intentionally thin and defer richer brand/card editing to the workspace itself.
- After signup, the merchant should land directly in their workspace.

### Workspace Information Architecture

- The merchant workspace must clearly separate:
  - `Brand`
  - `Loyalty Card`
  - `Customers`
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
- The wallet card preview should reflect the current merchant brand plus the current loyalty card template.
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
- Platform-level concerns such as `Billing`, `Account`, and `Log Out` should be available as subtle workspace utility actions, not as dominant navigation.

## Non-Goals

- Full auth and tenant isolation
- Production navigation design
- Persistent per-user merchant session selection
- Card-level design overrides
