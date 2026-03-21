# RFC-018 Onboarding Entry Focus

## Problem

The current onboarding routes reuse full portal top chrome, which introduces utility navigation and can dilute focus during account creation and conversion-critical steps.

## Decision

Add an onboarding header mode in the shared shell and apply it to signup, plan, and billing routes so these pages keep a guided, low-distraction top frame.

## Scope

- Shared shell enhancement for onboarding header mode.
- Apply mode to `/portal/signup`, `/portal/signup/plan/[merchantId]`, and `/portal/signup/billing/[merchantId]`.
- Keep roadmap and API contracts unchanged.

## Consequences

- Better conversion-oriented onboarding flow.
- Consistent public/workspace hierarchy remains intact.
- Lower UI entropy by reusing one shell with explicit context modes.
