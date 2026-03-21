# F34 Requirements

## Objective

Migrate critical merchant onboarding and workspace route slices to Next.js while preserving backend domain behavior through `/api/v1`.

## Functional Requirements

- Next.js must provide signup continuation routes for plan and billing after merchant creation.
- Billing route must complete owner credential setup via `/api/v1/session/complete-signup`.
- Workspace route must expose operate/configure/customers slices and call API operations for:
  - create programme from template
  - update programme details
  - award visit using scanned pass code
  - redeem customer rewards
- Frontend must support selecting the active programme context for workspace operations.

## Operational Requirements

- API integration tests must cover protected workspace and programme mutation flows.
- Existing merchant auth boundary tests must still pass.
