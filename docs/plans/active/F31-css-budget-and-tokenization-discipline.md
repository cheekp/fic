# F31 CSS Budget And Tokenization Discipline

## Goal
- Add enforceable stylesheet guardrails so UI polish can continue quickly without uncontrolled `app.css` growth or token drift.

## Scope
- Add a dedicated CSS budget validator script with thresholds for:
  - `app.css` line count and byte size
  - raw color literals outside the `:root` token block
  - minimum token declarations and `var(--...)` references
- Integrate CSS budget validation into `scripts/validate-ux-surface.sh`.
- Add targeted test coverage for CSS budget and token-discipline expectations.
- Update UX QA runbook with budget gate behavior and remediation guidance.
- Update architecture/RFC/spec/harness/README references for the new slice.

## Out Of Scope
- Splitting `app.css` into multiple files.
- Full screenshot diff baseline infrastructure.
- Re-theming the full UI.

## Proof
- `scripts/validate-f31-css-budget-and-tokenization.sh`
- `scripts/validate-ux-surface.sh`
