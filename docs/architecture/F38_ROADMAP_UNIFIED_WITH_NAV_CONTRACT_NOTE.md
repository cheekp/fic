# F38 Roadmap Unified With Nav Contract Note

## Intent

Unify roadmap/tube-map progression and portal rail state under one API payload so onboarding progression cannot drift across components.

## Decision

- Extend `PortalNavigationContract` with a `Roadmap` payload.
- Build roadmap steps in the same backend contract builder that assembles rail items and theme.
- Keep `OnboardingJourney` rendering-only by consuming API roadmap payload when available.

## Outcome

- Rail and roadmap now share one C# source of truth.
- Signup and workspace route slices consume one API object for shell + progression state.
