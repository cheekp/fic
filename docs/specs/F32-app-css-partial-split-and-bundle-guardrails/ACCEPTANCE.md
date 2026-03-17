# F32 Acceptance

- `src/Fic.Platform.Web/wwwroot/styles/app-auxiliary-surfaces.css` exists and is imported from `app.css`.
- `scripts/render-css-bundle.py` renders local `@import` chains into one CSS payload.
- `scripts/validate-css-budget.sh` evaluates bundle metrics using the renderer.
- `tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs` reads bundled CSS for UX style assertions.
- `scripts/validate-f32-app-css-partial-split.sh` passes.
- `scripts/validate-ux-surface.sh` passes.
