# F32 App CSS Partial Split Note

## Context

`app.css` has become a long-lived hotspot. Single-file edits increase review noise and make localized style changes harder to isolate.

## Decision

- Keep `app.css` as the single app entrypoint.
- Split auxiliary surface rules into partial files under `wwwroot/styles/`.
- Import partials from `app.css` via local `@import` statements.
- Evaluate CSS budgets against the rendered bundle (entry + imports) so guardrails remain meaningful after splitting.

## Consequences

- Style changes can be grouped by concern without changing app bootstrap behavior.
- Budget/token controls remain enforceable even when styles are distributed across files.
- Additional splits can happen incrementally as future slices prove stable seams.
