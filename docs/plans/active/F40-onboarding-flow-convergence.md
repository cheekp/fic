# F40 Onboarding Flow Convergence

## Goal

Make post-billing onboarding feel like one clear flow by aligning roadmap state and next-action surfaces in workspace.

## Scope

- Replace first-time workspace "Step 5/Step 6" onboarding cards with a single setup taskboard.
- Remove redundant step-number copy in workspace onboarding card headings.
- Add query-driven setup intent handling (`setup=shop`) to open shop setup modal directly.
- Route billing completion to workspace with setup intent query.
- Update Playwright workflow scripts for the new onboarding taskboard language/signals.

## Proof

- `npm run build` in `src/Fic.Platform.Frontend`
- `npm run qa:signup-flow` in `src/Fic.Platform.Frontend`
- `npm run qa:workspace-slices` in `src/Fic.Platform.Frontend`
- `scripts/validate-f40-onboarding-flow-convergence.sh`
