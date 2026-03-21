# RFC-015 Premium Foundation Query/Motion/Feedback

## Problem

Portal UX currently relies on manual fetch/state patterns and inline-only mutation feedback. This slows consistency, creates state lifecycle drift risk, and weakens perceived quality during repeated operator actions.

## Decision

Adopt a shared frontend interaction foundation:

- TanStack Query for server-state reads and cache lifecycle.
- Sonner for lightweight global success/error feedback.
- Vaul for mobile sheet-style utility navigation where applicable.
- Framer Motion for restrained shell/content transitions.

## Scope

- Add app-level Query provider and toaster host.
- Migrate portal navigation/workspace entry reads to Query hooks.
- Add toast feedback for key onboarding/workspace mutations.
- Introduce shell-level motion transition primitives.

## Consequences

- More predictable network state handling and easier progressive migration of remaining fetch paths.
- Reduced UI jitter/ambiguity during async actions.
- Better quality-of-feel without changing business-domain contracts.
