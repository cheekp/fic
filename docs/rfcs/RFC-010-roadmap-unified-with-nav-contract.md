# RFC-010 Roadmap Unified With Nav Contract

## Problem

Roadmap/tube-map progression and route rail state are maintained by separate frontend-derived logic paths, creating long-term drift risk.

## Decision

Add roadmap progression to `PortalNavigationContract` and consume this payload in `OnboardingJourney`.

## Scope

- Extend shared contracts with roadmap models.
- Backend builder emits roadmap for signup/workspace contexts.
- Frontend onboarding journey uses roadmap payload with legacy fallback retained.
- Extend integration tests to validate roadmap presence.

## Consequences

- One canonical progression model per page load.
- Lower UX entropy as roadmap and rail evolve together.
