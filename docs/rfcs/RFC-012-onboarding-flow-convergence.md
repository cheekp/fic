# RFC-012 Onboarding Flow Convergence

## Problem

After billing completion, workspace onboarding still feels fragmented: roadmap says one thing while the first action area can read like a separate flow, causing hesitation and extra clicks.

## Decision

Converge post-billing onboarding into a single action lane:

- roadmap remains the source of ordered progress
- a compact setup taskboard presents only the remaining actionable tasks
- billing handoff includes explicit setup intent in workspace URL so the first task is immediate

## Scope

- Replace workspace onboarding "Step 5/Step 6" card pattern with one setup taskboard.
- Remove redundant step-number copy from the workspace onboarding cards.
- Add `setup=shop` query handling in merchant workspace route to open shop setup modal intentionally.
- Update billing completion route to include setup intent query.
- Update QA scripts to assert new taskboard signals instead of old step-heading copy.

## Consequences

- Cleaner onboarding transition from signup to first-time operations.
- Lower cognitive load: one roadmap and one action panel, not parallel narratives.
