# F32 Requirements

## Objective
- Improve stylesheet maintainability by splitting `app.css` into partials without weakening UX guardrails.

## Functional Requirements
- `app.css` must remain the global entry stylesheet loaded by the app.
- `app.css` must import at least one partial stylesheet under `wwwroot/styles/`.
- Extracted styles must preserve current UX behavior for core rendered surfaces.
- CSS budget validation must evaluate the rendered global CSS bundle (entry + local imports), not only the entry file text.
- UX quality tests must read the rendered global CSS bundle when asserting token and motion contracts.
- F32 must provide a scriptable validator that checks split assets and runs the UX gate.

## Operational Requirements
- Existing validators for active UX slices must continue to pass.
- CSS budget thresholds remain enforceable after the split.
- Bundle rendering must fail clearly on missing import files or import cycles.
