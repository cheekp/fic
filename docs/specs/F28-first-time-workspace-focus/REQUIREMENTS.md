# F28 Requirements

## Objective
- The first merchant session must use one tight workflow that prioritizes first customer join and first stamp before exposing broader workspace complexity.

## Functional Requirements
- The workspace must enforce first-time route gating when:
  - the merchant has at most one programme
  - the shop has zero active customer cards
- This strict first-time route gating must run on parameter load so deep links are normalized before the user can continue in a secondary section.
- In first-time route gating mode, disallowed deep links must redirect to `Programmes` with one of:
  - `create`
  - `configure`
  - `operate`
- First-time primary navigation must show `Programmes` only.
- First-time programme sub-navigation must show `Operate` and `Configure` only.
- First-time `Operate` must hide secondary overview/timeline panels and show one clear next-step prompt.
- Advanced sections (`Overview`, `Insights`, `Customers`, programme `Insights`) must reappear after the first customer join exists.
- Shop settings must remain reachable while route gating is active.

## UX Requirements
- First-time copy must describe one immediate action without multi-step explanatory panels.
- The first-time surface must avoid duplicate onboarding guidance blocks.
- The default first-time `Operate` area must keep join and stamp actions above all secondary context.
