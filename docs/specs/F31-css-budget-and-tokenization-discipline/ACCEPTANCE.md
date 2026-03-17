# F31 Acceptance

- `scripts/validate-css-budget.sh` exists and validates `app.css` budgets.
- CSS budget validator fails when line/byte/literal thresholds are exceeded.
- CSS budget validator fails when token definition/reference minimums are not met.
- `scripts/validate-ux-surface.sh` runs CSS budget checks in the default fast gate path.
- `tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs` includes assertions for CSS budget/token discipline.
- `docs/runbooks/UX_QA_PLAYBOOK.md` documents CSS budget checks and expected remediation flow.
- `scripts/validate-f31-css-budget-and-tokenization.sh` passes.
