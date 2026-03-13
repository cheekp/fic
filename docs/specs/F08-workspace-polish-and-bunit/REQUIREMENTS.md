# F08 Workspace Polish And bUnit Requirements

## Objective

Reduce merchant-workspace clutter and add component-level test coverage for the current interaction model.

## Functional Requirements

### Workspace Polish

- merchant actions should appear in the section where they make sense rather than being duplicated across tabs
- customer join should remain card-specific and should not create duplicate primary actions in the card editor
- shop setup framing should stay visible through a checklist-style presentation rather than drifting back toward abstract status cards

### Component Coverage

- the repo must add bUnit-based component tests
- the first bUnit tests should cover current high-risk merchant UI behavior:
  - selected loyalty programme context
  - customer join action visibility and gating
  - shop setup/checklist rendering
- component tests should prefer focused assertions over brittle full-markup snapshots

## Non-Goals

- full end-to-end browser coverage
- visual regression tooling
- large-scale workspace redesign beyond the current flow
