# F39 Portal Utility IA And Workflow Polish

## Goal

Deliver a cleaner onboarding UX and establish utility destination IA in portal chrome.

## Scope

- Add utility links to portal header.
- Implement route surfaces for blogs/training/consultancy/account/billing.
- Simplify plan and billing screens by removing duplicate side summaries.
- Split billing into payment confirmation then owner credential setup within step 3.
- Promote owner access to a separate roadmap stage before billing confirmation.
- Remove workspace roadmap duplication by consolidating to one compact setup strip plus modal.
- Keep workspace roadmap state in sync with saved onboarding mutations.
- Remove completion check icon noise from workspace rail navigation.
- Remove workspace side rail and use inline section pills for Operate/Configure/Customers.
- Remove desktop rail-toggle control from portal chrome in merchant routes.
- Replace merchant shop setup right-edge drawer with centered modal workflow.
- Capture owner name at signup account step and carry it through the onboarding draft.
- Fix shop setup flow where logo upload and shop detail save could conflict.
- Ensure uploaded logos apply immediately by using cache-busting logo URLs.
- Ensure portal shell column layout does not reserve empty secondary space when no secondary panel is provided.
- Keep Playwright workflow QA running and capture updated screenshots.

## Proof

- `npm run build` in `src/Fic.Platform.Frontend`
- `npm run qa:signup-flow` in `src/Fic.Platform.Frontend`
- `npm run qa:workspace-slices` in `src/Fic.Platform.Frontend`
- `scripts/validate-f39-portal-utility-ia-and-workflow-polish.sh`
