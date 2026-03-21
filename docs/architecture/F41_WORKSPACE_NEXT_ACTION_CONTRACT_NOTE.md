# F41 Workspace Next-Action Contract Note

## Intent

Make post-billing workspace onboarding read as a single, API-owned flow by exposing one explicit next action contract from the backend.

## Decisions

- Add `nextAction` to portal navigation contracts.
- Keep `roadmap` for ordered progress and use `nextAction` for immediate operator intent.
- Model `nextAction` with:
  - key
  - title
  - summary
  - cta label/href
  - optional blocked reason
  - compact task list (complete/required/blocked)
- Build workspace `nextAction` in C# from setup checklist state:
  - shop setup required first
  - then programme creation
  - no nextAction once onboarding completion criteria are met
- Render onboarding taskboard in Next.js from contract payload first, with local fallback only for resilience.

## Outcome

- One source of truth for “what to do next” after signup.
- Fewer contradictory progress cues between roadmap, taskboard, and section routing.
