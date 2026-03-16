# F30 UX Quality Gate Architecture Note

## Context

F30 introduced heavy onboarding and workspace polish. The main risk now is visual regression and UX drift as the UI evolves quickly.

## Decision

Adopt a three-layer UX quality gate for the Blazor UI:

1. Component contract tests (bUnit)
   - enforce key UX hierarchy rules (shared segmented controls, CTA priority, metadata density).
2. Style guard tests
   - ensure control tokens and reduced-motion fallback remain present in the shared stylesheet.
3. Optional browser smoke (Playwright)
   - run viewport overflow checks and output screenshots for key surfaces.

## Why this shape

- Fast enough for routine PR use.
- Strict enough to catch recurrent UX consistency regressions.
- Extensible for additional states without requiring full end-to-end infrastructure first.

## Implementation surface

- Tests: `tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs`
- Validator: `scripts/validate-ux-surface.sh`
- Runbook: `docs/runbooks/UX_QA_PLAYBOOK.md`

## Consequences

- UI changes now have a predictable gate beyond manual review.
- Optional browser smoke can be enabled when visual risk is higher (`FIC_UX_BROWSER_SMOKE=1`).
- Screenshot evidence is available under `artifacts/ux-smoke/` when browser smoke runs.
