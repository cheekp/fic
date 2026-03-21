# F41 Requirements

## Functional

1. Portal navigation contract supports an optional `nextAction` payload for onboarding/action intent.
2. Workspace portal navigation endpoint returns `nextAction` when setup checklist is incomplete.
3. Workspace `nextAction` is determined in C# domain/application layer from setup checklist state.
4. Workspace onboarding taskboard in Next.js renders title, summary, CTA, and task statuses from `nextAction` when present.
5. Workspace onboarding taskboard keeps shop-setup modal trigger behavior when `nextAction.key` is `shop`.
6. Workspace onboarding taskboard keeps first-programme creation flow when `nextAction.key` is `programme`.
7. API tests verify `nextAction` is returned for authenticated workspace onboarding states.

## Non-Functional

1. Existing roadmap payload remains backward compatible for current clients.
2. Frontend retains local fallback behavior when `nextAction` is absent.
