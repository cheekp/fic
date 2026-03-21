# RFC-011 Portal Utility IA And Workflow Polish

## Problem

Onboarding screens had too many competing elements and no clear placement for non-core destinations like training, consultancy, or account support.

## Decision

Add utility IA destinations and simplify onboarding layouts to prioritize one decision/action path per screen.

## Scope

- Portal shell utility links in top chrome.
- Next.js utility route surfaces: `/blogs`, `/training`, `/consultancy`, `/account`, `/billing`.
- Remove redundant side panes from signup plan and billing routes.
- Split billing route into explicit payment confirmation and owner credential sub-steps.
- Treat owner access as its own roadmap stage before billing.
- Capture owner name at signup account step for consistent owner identity context downstream.
- Remove workspace rail completion tick icons and keep roadmap as the canonical progress visual.
- Remove the persistent workspace side rail in favor of compact inline section navigation.
- Remove desktop rail-toggle chrome control in merchant flow to prevent accidental narrow-column layouts.
- Use centered modal composition for merchant shop setup instead of edge drawer behavior.
- Keep roadmap payload and visible current stage synchronized after onboarding mutations.
- Ensure brand logo uploads invalidate stale cached logo URLs in Next.js surfaces.
- QA screenshots via Playwright for onboarding and utility routes.

## Consequences

- Better navigation scalability for future app surfaces.
- Less visual noise in critical onboarding steps.
