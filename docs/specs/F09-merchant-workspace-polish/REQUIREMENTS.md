# F09 Merchant Workspace Polish Requirements

## Objective

Reduce merchant-workspace noise and make the page structure match the real product model more closely.

## Functional Requirements

### Workspace Hierarchy

- FIC-level account and utility actions should stay visually separate from merchant-owned workspace content
- the merchant workspace must present two primary scopes:
  - `Shop`
  - `Programmes`
- the overall page should read from top to bottom as:
  - FIC utility chrome
  - merchant shop context
  - selected workspace scope
- a loyalty card must be presented as a tool or artifact of a loyalty programme, not as the top-level object name

### Shop Scope

- the `Shop` scope should contain:
  - `Overview`
  - `Edit Shop`
  - `Insights`
- shop details and branding should stay editable only inside the shop area
- setup readiness should be consolidated into a horizontal roadmap/progress presentation rather than abstract status cards
- the roadmap should support collapse and dismiss behavior without breaking the rest of the workspace layout
- once setup is complete, the roadmap should feel secondary to the working interface

### Programme Scope

- the `Programmes` scope should make it obvious that a shop can manage multiple loyalty programmes
- the selected programme must expose:
  - `Operate`
  - `Configure`
  - `Insights`
- `Operate` should own customer join, QR sharing, till-side stamping, and issued customer passes
- `Configure` should own the programme rule, begin and expiry dates, and wallet card copy
- the wallet preview should stay programme-specific and inherit from the shop brand by default
- the daily-use/default working area after setup should favour programme operations over shop editing

### Insights

- insights must distinguish between shop-wide metrics and selected-programme metrics
- the terminology should stay merchant-friendly and avoid internal jargon where possible
- shop insights and programme insights must not be mixed into a single peer tab view

## Non-Goals

- full visual redesign of the entire product shell
- auth, billing, or support-bot implementation
- browser-based end-to-end coverage
