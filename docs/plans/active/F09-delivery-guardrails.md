# F09 Delivery Guardrails

## Goal

Add lightweight, executable guardrails that reduce avoidable rework in UI-heavy slices.

## Why Now

- workspace-focused slices showed repeated polish churn in the same files
- component interaction coverage improved only after flow refactors were already deep
- local reliability fixes consumed implementation time in earlier slices

## Scope

- add a local preflight script for launch/config/static-asset and SSR form checks
- add a churn hotspot reporting script to identify repeated change concentration
- add a seam checklist template for cross-surface slices
- add a validator for this slice and update harness guidance

## Planned Outcomes

- reusable `scripts/dev-preflight.sh`
- reusable `scripts/report-churn-hotspots.sh`
- seam checklist template under `docs/specs/`
- `scripts/validate-f09-delivery-guardrails.sh`

## Exit Criteria

- F09 requirements and acceptance docs exist
- new scripts run successfully from repo root
- F09 validator passes
- harness includes the new local proof loop and guardrail references
