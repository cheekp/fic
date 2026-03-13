# F09 Merchant Workspace Polish Requirements

## Objective

Reduce merchant-workspace noise and make the page structure match the real product model more closely.

## Functional Requirements

### Shop Area

- the shop tab should act as the owner-facing setup and profile area for the coffee shop
- shop details and branding should stay editable there
- setup readiness should be consolidated into a horizontal roadmap/progress presentation rather than abstract status cards
- the roadmap should support collapse and dismiss behavior without breaking the rest of the workspace layout

### Workspace Hierarchy

- FIC-level account and utility actions should stay visually separate from merchant-owned workspace content
- the merchant workspace should present shop-level meta before dropping the user into loyalty-programme operations
- the overall page should read from top to bottom as:
  - FIC utility chrome
  - merchant shop meta and onboarding state
  - loyalty card and customer operations

### Loyalty Cards

- the cards tab should make it obvious that a shop can manage multiple loyalty cards/programmes
- the selected card editor should focus on programme configuration, validity dates, and wallet copy
- the wallet preview should stay card-specific and inherit from the shop brand by default

### Customers

- customer join and till-side stamping must remain tied to the selected loyalty card
- the customer area should make the selected-card context obvious without duplicating card-editing controls

### Insights

- insights must distinguish between shop-wide metrics and selected-card metrics
- the terminology should stay merchant-friendly and avoid internal jargon where possible

## Non-Goals

- full visual redesign of the entire product shell
- auth, billing, or support-bot implementation
- browser-based end-to-end coverage
