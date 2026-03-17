# RFC-003 CSS Budget And Tokenization Discipline

## Problem

Global stylesheet churn is recurring across UX slices. Without explicit budgets and token checks, UI polish changes can ship with avoidable growth and raw color literal spread.

## Context

- `app.css` is a repeat hotspot in recent PRs.
- UX quality gates exist, but they focus mainly on interaction contracts and reduced-motion coverage.
- Review comments often request late hardening for consistency and control styling.

## Options Considered

1. Keep manual CSS review only
   - low setup, inconsistent outcomes.
2. Full visual diff baseline pipeline now
   - strong coverage, high setup overhead for current iteration speed.
3. Budget + token discipline gate in the existing UX validator
   - low overhead, enforceable guardrails, immediate impact.

## Decision

Adopt option 3.

Add a dedicated script that enforces:
- line-count and byte-count budgets for `app.css`
- raw color-literal budget outside `:root`
- minimum token definitions and token references (`var(--...)`)

Wire this script into `scripts/validate-ux-surface.sh` and maintain thresholds in source control.

## Consequences

- CSS growth becomes a conscious tradeoff instead of accidental drift.
- Teams must convert repeated literals into tokens before landing large visual changes.
- Budget exceptions require updating docs/specs/RFC together, improving review clarity.

## Open Questions

- Should future slices split `app.css` by surface to tighten budgets further?
- When CI matures, should budget checks become blocking on all branches or only UX-labeled PRs?
