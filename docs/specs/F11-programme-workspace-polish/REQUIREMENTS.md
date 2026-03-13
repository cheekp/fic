# F11 Programme Workspace Polish Requirements

## Objective

Polish the merchant workspace so the current `Shop -> Programmes -> Programme section` hierarchy feels obvious, calm, and efficient in daily use.

## Functional Requirements

### Shop Settings Flow

- shop settings must remain available from the merchant workspace without acting like a competing primary page
- the main shop navigation should stay focused on:
  - `Overview`
  - `Programmes`
  - `Insights`
- shop settings should open as a lighter in-context surface
- existing deep links that target the old edit route should continue to reach shop settings without breaking

### Programme Rail

- the left side of `Shop -> Programmes` should behave like a programme rail, not a second content page
- the programme rail must keep:
  - grouped lifecycle sections
  - selected programme indication
  - `New Programme`
- the programme rail should avoid large explainer copy once grouped programme selection is visible
- creating a programme must still make the current delivery option explicit

### Programme Context Bar

- the selected programme header should act as a compact context bar
- it should show only the minimum selected-programme context needed to orient the merchant:
  - reward headline
  - validity state
  - validity window
  - current delivery output
- it should not repeat a summary dashboard already visible elsewhere

### Programme Operate

- `Operate` must remain the default daily-use area
- `Operate` should continue to expose:
  - customer join QR
  - join link
  - till-side stamping
  - issued customer passes
  - recent programme activity
- `Operate` should not depend on explanatory copy to clarify what it is for

### Programme Configure

- `Configure` should feel like one work canvas
- `Configure` must keep:
  - programme rule
  - begin date
  - expiry date
  - current delivery output
  - customer-facing delivery copy
  - live preview
- the preview should stay visible, but the configure surface should avoid redundant explanatory panels that restate the same model

### Naming

- `programme` remains the governing object for loyalty operations
- `wallet loyalty card` remains the current delivery output of a programme
- the UI should not imply that creating a programme is abstract while silently creating a wallet-card flow

## Non-Goals

- auth or billing implementation
- new delivery outputs beyond the current wallet loyalty card option
- major redesign of the FIC utility chrome
