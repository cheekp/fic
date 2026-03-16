# RFC-002 UX Quality Gates For Fast UI Iteration

## Problem

UI polish changes are shipping quickly, but review feedback is repeatedly finding consistency, density, and layout issues late. Manual review alone is too slow and too variable for the desired quality bar.

## Context

- FIC relies on rapid UI iteration for onboarding and workspace flow.
- Existing validators primarily protect domain and component behavior.
- We need lightweight UX safeguards that can run on every PR.

## Options Considered

1. Manual review only
   - simple but inconsistent and slow.
2. Full browser E2E suite for every change
   - high confidence but too expensive for daily iteration right now.
3. Layered UX gate (component contracts + style guards + optional browser smoke)
   - balanced cost/coverage with room to scale.

## Decision

Adopt option 3.

Use:
- bUnit-based UX contract tests for hierarchy and interaction patterns.
- stylesheet guard checks for shared tokens and motion/accessibility fallbacks.
- optional Playwright smoke checks for viewport overflow and screenshot evidence.

## Consequences

- Faster feedback loops for UI regressions.
- Better consistency across pages and states without relying on memory.
- Optional browser checks can be enabled for higher-risk visual changes without slowing all commits.

## Open Questions

- When CI checks are enabled for PRs, should browser smoke become mandatory on selected branches?
- Should we add per-surface baseline image diffs once core flows stabilize further?
