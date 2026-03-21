# F40 Onboarding Flow Convergence Note

## Intent

Reduce post-billing onboarding confusion by converging roadmap, next action, and workspace entry behavior into one clear flow.

## Decisions

- Keep the roadmap as the canonical progress visual.
- Replace the separate "Step 5/Step 6" workspace onboarding cards with a single setup taskboard.
- Remove redundant step-number copy from workspace onboarding cards where roadmap already provides ordered progress.
- Keep one primary CTA in the setup taskboard based on true next action:
  - `Open shop setup` when shop details are incomplete
  - `Create programme` when shop details are complete but no programme exists
- Support query-driven setup intent in workspace (`setup=shop`) so modal entry is explicit and scriptable.
- Route billing completion directly to workspace with setup intent query so operators land on the next required action immediately.
- Keep shop setup as centered modal composition and preserve current logo upload + save flow behavior.

## Outcome

- Fewer competing onboarding signals after billing handoff.
- Clearer transition from billing completion to first required merchant action.
- Less vertical dead space in first-run workspace surfaces.
