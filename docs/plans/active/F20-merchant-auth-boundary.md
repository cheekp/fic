# F20 Merchant Auth Boundary

## Goal
- Give the merchant workspace a real ownership boundary by introducing merchant credentials during onboarding, a cookie-backed merchant session, and route-level access checks for merchant-owned pages.

## Scope
- Add merchant access credentials to the onboarding flow.
- Replace the login stub with a working merchant sign-in path.
- Add merchant sign-out and access-denied handling.
- Require a signed-in merchant session to access merchant-owned workspace routes.
- Ensure the signed-in merchant can only open their own workspace.
- Prove the flow with automated tests and a dedicated validator.

## Out of Scope
- Full production identity infrastructure.
- Password reset delivery beyond stub messaging.
- Role-based staff accounts or multi-user merchant teams.
- Cross-device session management beyond the current in-memory baseline.

## Proof
- `scripts/validate-f20-merchant-auth-boundary.sh`
