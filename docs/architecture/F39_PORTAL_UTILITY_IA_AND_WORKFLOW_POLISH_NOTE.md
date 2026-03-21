# F39 Portal Utility IA And Workflow Polish Note

## Intent

Stabilize signup/workspace workflow readability and add a durable utility information architecture layer for non-core destinations.

## Decisions

- Keep onboarding surfaces focused on one primary action and one roadmap.
- Remove redundant right-side summary panes from plan and billing.
- Split billing and owner credential setup into explicit sub-steps in the billing route.
- Elevate owner access as its own roadmap stage before billing confirmation.
- Capture owner name earlier in signup so billing/owner surfaces can show consistent identity context.
- Remove completion check icons from workspace rail to reduce redundant status signaling.
- Remove the pinned merchant side rail in workspace and replace with a lighter inline section switcher.
- Remove the desktop rail toggle control from portal chrome to avoid non-functional layout churn.
- Replace merchant shop setup edge-drawer behavior with a centered modal to keep workflow context stable.
- Ensure roadmap state is refreshed after onboarding mutations so current step and completion stay aligned.
- Ensure uploaded logo assets use cache-busting URLs so branding applies immediately across Next.js surfaces.
- Add portal-available utility destinations for:
  - Blogs
  - Training
  - Consultancy
  - Account
  - Billing
  - Log out
- Expose utility links consistently from portal chrome.

## Outcome

- Cleaner onboarding composition with fewer competing panels.
- Clear place in the product shell for company/support destinations as the portal expands.
- Shop setup save and logo upload no longer block each other through shared loading state.
