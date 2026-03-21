# F44 Owner/Billing + Setup Lane Cleanup Note

## Intent

Reduce onboarding friction by making owner access and billing progression explicit, then carrying users into a single, non-duplicated setup lane in workspace.

## Decisions

- Treat owner credentials and billing confirmation as two explicit sub-steps inside signup billing flow.
- Remove redundant explanatory copy and duplicated progress messaging in onboarding surfaces.
- In workspace onboarding mode, show one primary next-action card instead of stacked taskboard complexity.
- Keep setup blade as the fast path for shop details and brand logo update.

## Outcome

- Cleaner mental model: Owner access -> Billing -> Shop setup -> Programme template.
- Less UI clutter and fewer conflicting cues between roadmap and card content.
- Faster first-time operator completion through one clear next action.
