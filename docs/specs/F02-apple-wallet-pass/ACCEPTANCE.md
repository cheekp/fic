# F02 Apple Wallet Pass Acceptance

## Acceptance Checks

1. Run `./scripts/validate-f02-apple-wallet-pass.sh`.
2. Review the pass-generation contract against Apple Wallet package expectations.
3. Confirm the join flow can choose between preview fallback and `.pkpass` download.

## Acceptance Criteria

- `F02-ACC-001` The repo contains implementation-ready requirements for Apple Wallet pass generation and fallback behavior.
- `F02-ACC-002` The repo contains a contract for Apple Wallet pass configuration and minimum pass fields.
- `F02-ACC-003` The `WalletPasses` codebase contains a dedicated service seam for Apple Wallet package generation.
- `F02-ACC-004` The web layer exposes a pass-download route for a customer card.
- `F02-ACC-005` The join flow uses preview fallback when signing is unavailable.
- `F02-ACC-006` The validation script builds the solution and checks the wallet issuance seam exists.

## Evidence To Capture Once Certificates Exist

- `Add to Wallet` downloads a valid `.pkpass`
- Safari on iPhone offers the Apple Wallet add flow
- the pass renders branded merchant details
- the barcode message matches the vendor scan code
- the pass shows progress such as `0/5 coffees`
- when signing is disabled, the join flow still routes to the preview card instead of erroring
