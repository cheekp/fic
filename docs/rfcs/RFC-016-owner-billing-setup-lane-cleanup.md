# RFC-016 Owner/Billing Setup Lane Cleanup

## Problem

Current onboarding still presents multiple overlapping hints and setup surfaces, causing confusion after billing and diluting the roadmap as the source of progression truth.

## Decision

Refactor the UX flow to:

- make owner access and billing progression explicit and compact in signup billing,
- simplify workspace onboarding into one next-action lane,
- preserve setup blade editing for shop details/logo as the primary completion path.

## Scope

- Compact owner/billing step indicator and stage-specific content framing.
- Remove redundant post-billing setup text blocks in workspace onboarding mode.
- Replace setup taskboard stack with one focused next-action card.
- Keep roadmap contract-driven and avoid changing API route contracts.

## Consequences

- Improved completion clarity and lower cognitive load.
- Less visual duplication between roadmap and card content.
- Easier future evolution of onboarding surfaces with one dominant action lane.
