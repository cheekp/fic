# F31 CSS Budget And Token Discipline Note

## Context

Recent slices repeatedly landed large stylesheet deltas in `src/Fic.Platform.Web/wwwroot/app.css`, often alongside onboarding and workspace behavior changes. This slows review and increases visual regression risk because style growth and token drift are not explicitly gated.

## Decision

Add a CSS budgeting and token-discipline gate to the existing UX validation loop.

The gate must enforce:

1. File-size budget ceilings for `app.css`
2. Raw color-literal budget ceilings outside the `:root` token declaration block
3. Minimum token definition and token-reference presence
4. Explicit review messaging when budgets are exceeded

## Why this shape

- Keeps feedback fast: plain shell validation runs in local loops and PR checks.
- Keeps design intent durable: token usage stays enforceable, not advisory.
- Avoids waiting for full visual-diff infrastructure before enforcing stylesheet discipline.

## Implementation Surface

- Budget validator: `scripts/validate-css-budget.sh`
- UX gate integration: `scripts/validate-ux-surface.sh`
- Test coverage: `tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs`
- Operator workflow: `docs/runbooks/UX_QA_PLAYBOOK.md`

## Consequences

- PRs that bloat global CSS or bypass tokens fail early.
- UI refactors should prefer token reuse and component-scoped styles over app-level growth.
- Budget thresholds can evolve deliberately by updating this note, RFC, and spec in the same slice.
