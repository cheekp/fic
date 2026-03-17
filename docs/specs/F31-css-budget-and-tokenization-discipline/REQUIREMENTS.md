# F31 Requirements

## Objective
- Enforce CSS growth limits and token discipline so polish slices stay reviewable and consistent.

## Functional Requirements
- A scriptable validator must enforce budget checks against `src/Fic.Platform.Web/wwwroot/app.css`.
- Budget checks must include:
  - maximum line count
  - maximum byte count
  - maximum raw color-literal count outside `:root`
- Token discipline checks must include:
  - minimum number of token definitions in `:root`
  - minimum number of `var(--...)` references in stylesheet rules
- `scripts/validate-ux-surface.sh` must call the CSS budget validator as part of the fast gate.
- A slice-level validator for F31 must verify docs/spec references and run the UX gate.

## UX Requirements
- CSS changes should prefer token reuse over introducing new hard-coded literals.
- Budget failures must give clear remediation hints so PR authors can fix quickly.

## Operational Requirements
- F31 must keep existing UX quality gate tests passing.
- Budget thresholds must be versioned in repo code and reviewed like any behavior change.
- Runbook guidance must document when to run the gate and how to respond to failures.
