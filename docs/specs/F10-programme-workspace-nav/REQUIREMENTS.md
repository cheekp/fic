# F10 Programme Workspace Navigation Requirements

## Objective

Make the merchant workspace easier to operate by treating the shop as the root context, programmes as the operational layer inside it, and the loyalty card as a programme-level output.

## Functional Requirements

### Merchant Navigation

- the merchant workspace should treat `Shop` as the root context
- `Programmes` must sit under the shop workspace rather than acting as a peer root scope
- when a merchant has at least one programme, the default workspace landing should favour `Shop -> Programmes -> Operate`
- FIC utility chrome should stay present but visually subordinate to merchant navigation

### Shop Scope

- the `Shop` scope should remain the place for:
  - merchant profile
  - branding
  - shop-wide insights
- the shop overview should avoid repeating selected-programme controls, join actions, or daily-use customer operations
- the shop overview should use a small number of summary panels and direct actions, not a second operational dashboard
- shop editing should remain available, but as a secondary settings action rather than a primary working tab
- the workspace should avoid duplicated actions such as multiple links to the same shop-settings destination
- onboarding guidance may remain on the shop overview, but it should stay secondary once complete

### Programme Scope

- the `Programmes` section should feel like a dedicated working area for the selected programme
- the `Programmes` section should use a narrow selection rail on the left and one focused work surface on the right
- creating a programme should make the current delivery option explicit instead of implying a generic empty programme while silently creating a wallet-card flow
- the programme list must be grouped by lifecycle state:
  - `Active`
  - `Scheduled`
  - `Expired`
- selecting a programme must preserve the nested programme section where practical
- the selected programme area must continue to expose:
  - `Operate`
  - `Configure`
  - `Insights`
- the selected programme header should stay compact and should not repeat a large summary dashboard already available in the nested sections
- the programmes layout should prioritize programme selection and the active nested view over large explanatory banners
- the programmes rail should avoid large explainer blocks once grouped programme selection is visible

### Programme Operate

- `Operate` should remain the default daily-use area for a selected programme
- `Operate` should own:
  - customer join QR
  - join link
  - till-side stamping
  - issued customer passes
  - recent activity for the selected programme

### Programme Configure

- `Configure` should own:
  - programme reward rule
  - begin date
  - expiry date
  - current customer delivery copy
  - current customer delivery preview
- `Configure` must communicate that the loyalty card is the current customer-facing output of the programme, not the definition of the programme itself
- `Configure` should make delivery management visible enough that future delivery options could be added without renaming the core programme model
- the configuration surface should not repeat customer join or stamping actions
- the configuration surface should avoid separate explanatory panels once the delivery option and preview are already visible

### Programme Insights

- `Programmes -> Insights` must show only selected-programme metrics and programme activity context
- `Shop -> Insights` must show only shop-level metrics and programme comparison context

### Naming

- UI copy should prefer `programme` as the governing object for loyalty operations
- `loyalty card` should only appear as the current customer-facing output or option within a programme
- the interface should avoid language that makes `loyalty card` sound like a peer to `shop` or `programme`

## Non-Goals

- auth or billing implementation
- backend model changes for programme lifecycle beyond what can be derived from existing dates
- full visual redesign of the whole application shell
