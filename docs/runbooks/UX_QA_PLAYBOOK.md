# UX QA Playbook

Use this playbook to keep UI changes reviewable, consistent, and fast to verify.

## Fast Gate (every PR)

Run:

```bash
./scripts/validate-ux-surface.sh
```

What it checks:
- UX contract tests for shared navigation patterns and CTA hierarchy.
- Style-token and reduced-motion safeguards in `app.css`.
- Selected-programme metadata density constraints for workspace focus.

## Optional Browser Smoke (before design-heavy merges)

Run:

```bash
FIC_UX_BROWSER_SMOKE=1 ./scripts/validate-ux-surface.sh
```

What it adds:
- Playwright browser pass that renders key surfaces and checks horizontal overflow at core breakpoints.
- Screenshot artifacts under `artifacts/ux-smoke/` for rapid visual review.

## Suggested Review Rhythm

1. Run the fast gate after each meaningful UI commit.
2. Run browser smoke before opening a UX-focused PR or before merge.
3. Attach screenshots for the key desktop and mobile states when visual changes are material.
4. Keep fixes in small slices so regressions are easy to isolate.
