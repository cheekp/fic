# F18 Real Wallet Demo Acceptance

## Acceptance Criteria

1. The active plan is `F18-real-wallet-demo`, and `F17` has moved to completed plans.
2. The merchant workspace shows a clear preview-fallback state with a path to Wallet demo setup help when signing is not ready.
3. The app exposes a Wallet demo support page that lists current readiness issues.
4. The LAN demo script and Wallet runbook both point to the readiness surface.
5. Automated tests generate a signed `.pkpass` archive using generated certificates and verify the package structure.
6. Updated tests pass.
7. The F18 validator passes.

## Demo Walkthrough

1. Open `/support/wallet-demo` without signing configured and confirm the page explains preview fallback plus missing inputs.
2. Open a merchant workspace and confirm it surfaces `Wallet demo setup` while in preview mode.
3. Configure signing material locally and confirm the capability changes to signed Wallet readiness.
4. Run the F18 validator and confirm build plus tests pass.
