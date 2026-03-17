# RFC-004 App CSS Partials And Bundled Budgeting

## Problem

A single large `app.css` slows iterative review and creates recurring same-file churn, but naive splitting can bypass budget checks that only inspect one file.

## Decision

Adopt a staged split model:
- keep `app.css` as the entry stylesheet
- move stable sections into partials imported by `app.css`
- compute budget/token metrics from the rendered global bundle

## Consequences

- Better maintainability with lower merge friction.
- Guardrails remain strict after file decomposition.
- Future splitting can proceed in bounded increments without weakening quality gates.
