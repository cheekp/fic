# RFC-013 Workspace Next-Action Contract

## Problem

Workspace onboarding still relies on mixed local heuristics and UI-specific logic to determine the immediate next task, which risks drift from backend setup state.

## Decision

Expose a backend-owned `nextAction` contract on portal navigation payloads and make workspace onboarding action surfaces render from that contract.

## Scope

- Extend portal navigation contract with `nextAction`.
- Build workspace next action in C# from merchant setup checklist.
- Update Next.js workspace onboarding taskboard to read from contract first.
- Keep roadmap contract as canonical sequence and `nextAction` as canonical immediate action.
- Expand API tests to validate `nextAction` presence and key for incomplete setup states.

## Consequences

- Stronger DDD boundary: action intent comes from domain/application state, not view-only logic.
- Safer UX iteration: taskboard refinements no longer change progression rules.
