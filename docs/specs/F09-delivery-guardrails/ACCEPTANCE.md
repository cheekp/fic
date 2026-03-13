# F09 Delivery Guardrails Acceptance

## Acceptance Criteria

1. `scripts/dev-preflight.sh` exists and exits successfully in the current repo state.
2. `scripts/report-churn-hotspots.sh` exists and outputs ranked file churn for a bounded history window.
3. `docs/specs/SEAM_CHECKLIST_TEMPLATE.md` exists and includes seam ownership fields.
4. `scripts/validate-f09-delivery-guardrails.sh` validates this slice end-to-end.
5. `docs/ENGINEERING_HARNESS.md` references preflight and churn reporting as part of the local quality loop.

## Demo Walkthrough

1. Run `scripts/dev-preflight.sh`.
2. Run `scripts/report-churn-hotspots.sh 60 10`.
3. Open `docs/specs/SEAM_CHECKLIST_TEMPLATE.md` and confirm required seam fields.
4. Run `scripts/validate-f09-delivery-guardrails.sh`.
5. Confirm the harness local proof loop includes preflight and hotspot checks.
