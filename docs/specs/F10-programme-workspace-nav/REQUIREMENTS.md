# F10 Programme Workspace Navigation Requirements

## Objective

Make the merchant workspace easier to operate by centering the selected programme and reducing visual competition between shop, programme, and platform concerns.

## Functional Requirements

### Merchant Navigation

- the merchant workspace should keep `Shop` and `Programmes` as the primary scopes
- when a merchant has at least one programme, the default workspace landing should favour `Programmes -> Operate`
- FIC utility chrome should stay present but visually subordinate to merchant navigation

### Shop Scope

- the `Shop` scope should remain the place for:
  - merchant profile
  - branding
  - shop-wide insights
- the shop overview should avoid repeating selected-programme controls or daily-use customer operations
- onboarding guidance may remain on the shop overview, but it should stay secondary once complete

### Programme Scope

- the `Programmes` scope should feel like a dedicated working area for the selected programme
- the programme list must be grouped by lifecycle state:
  - `Active`
  - `Scheduled`
  - `Expired`
- selecting a programme must preserve the nested programme section where practical
- the selected programme area must continue to expose:
  - `Operate`
  - `Configure`
  - `Insights`

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
  - wallet card copy
  - wallet preview
- the configuration surface should not repeat customer join or stamping actions

### Programme Insights

- `Programmes -> Insights` must show only selected-programme metrics and programme activity context
- `Shop -> Insights` must show only shop-level metrics and programme comparison context

## Non-Goals

- auth or billing implementation
- backend model changes for programme lifecycle beyond what can be derived from existing dates
- full visual redesign of the whole application shell
