# F38 Requirements

## Functional

1. `PortalNavigationContract` includes a roadmap progression payload.
2. Signup nav endpoint includes roadmap with steps: account, plan, billing, shop, programme.
3. Workspace nav endpoint includes roadmap derived from setup checklist and programme readiness.
4. `OnboardingJourney` uses API roadmap payload when supplied.
5. Existing pages preserve behavior with fallback when roadmap payload is unavailable.

## Non-Functional

1. Frontend no longer owns canonical roadmap progression logic when API payload is present.
2. Contract fields remain typed in shared contracts.
