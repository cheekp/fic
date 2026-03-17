# F32 App CSS Partial Split And Bundle Guardrails

## Goal
- Split `app.css` into partial files so styling work is easier to review, while keeping CSS budget/token checks enforceable across the full loaded bundle.

## Scope
- Extract a stable auxiliary surface section from `src/Fic.Platform.Web/wwwroot/app.css` into `src/Fic.Platform.Web/wwwroot/styles/app-auxiliary-surfaces.css`.
- Keep `app.css` as the global stylesheet entrypoint and import extracted partials from it.
- Add a CSS bundle renderer utility and update budget checks to evaluate bundled CSS content (entry file + imports).
- Update UX quality tests to validate bundled CSS content so browser smoke and budget tests remain accurate.
- Add a slice validator for F32 and update docs/indexes/harness references.

## Out Of Scope
- Full decomposition of all CSS by feature area.
- Introducing a CSS preprocessor build pipeline.

## Proof
- `scripts/validate-f32-app-css-partial-split.sh`
- `scripts/validate-ux-surface.sh`
