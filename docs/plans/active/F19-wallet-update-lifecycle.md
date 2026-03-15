# F19 Wallet Update Lifecycle

## Goal
- Implement the Apple Wallet web-service update path so an issued pass can register, discover changes after stamping, and fetch a refreshed `.pkpass`.

## Scope
- Add Wallet web-service endpoints for registration, unregistration, updated serial discovery, updated pass retrieval, and Wallet log intake.
- Add in-memory demo registration state for Wallet devices and pass subscriptions.
- Include `webServiceURL` and `authenticationToken` in signed pass generation.
- Ensure merchant actions that change pass state are visible immediately through the update lifecycle.
- Prove the full path with automated tests and a dedicated slice validator.

## Out of Scope
- Production APNs hardening beyond a founder-demo seam.
- Merchant auth and tenant isolation beyond the current demo boundary.
- Alternative programme delivery outputs beyond the current Wallet card path.

## Proof
- `scripts/validate-f19-wallet-update-lifecycle.sh`
