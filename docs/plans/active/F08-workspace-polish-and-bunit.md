# F08 Workspace Polish And bUnit

## Goal

Tighten the merchant workspace experience and add component-level UI coverage around the highest-risk interactions.

## Why Now

- the merchant flow is materially better, but still easy to regress during layout cleanup
- state-level tests now exist, but the repo still lacks component coverage for workspace rendering and action gating
- the next UI slice should reduce clutter while proving the merchant experience behaves correctly at the component level

## Scope

- simplify the merchant workspace where duplicated or noisy actions confuse the flow
- keep customer join actions clearly card-specific
- add bUnit-based component tests around merchant workspace and join behavior
- keep the test surface focused on current high-risk interactions rather than broad snapshot coverage

## Planned Outcomes

- cleaner `VendorWorkspace` interaction model
- component tests that prove:
  - scheduled or expired cards do not present active join affordances in the wrong places
  - selected programme context drives the visible customer join surface
  - the setup/checklist framing remains visible in the shop area
- a validator for the slice

## Exit Criteria

- `F08` spec exists with explicit requirements and acceptance
- the workspace has at least one meaningful clutter reduction
- bUnit is wired into the test project
- component tests pass alongside the existing state tests
- the slice has a scriptable validator
