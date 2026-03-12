# F01 Internal MVP Demo Acceptance

## Acceptance Checks

1. Run `./scripts/validate-f01-internal-mvp-demo.sh`.
2. Confirm the internal MVP/demo documentation set is present and internally consistent.
3. Review the first-slice flow against the business plan and architecture draft.

## Acceptance Criteria

- `F01-ACC-001` The repo contains an RFC describing the platform architecture decision for the internal MVP/demo.
- `F01-ACC-002` The repo contains implementation-ready requirements for vendor sign-up, configuration, QR join, Apple Wallet issuance, and progress state.
- `F01-ACC-003` The spec explicitly requires visible wallet progress state such as `2/5 coffees`.
- `F01-ACC-004` The spec explicitly requires the vendor PWA and wallet pass to share the same progress truth.
- `F01-ACC-005` The spec explicitly defines validation for the slice.
- `F01-ACC-006` The architecture draft identifies SignalR, Apple Wallet update flow, KPI separation, and Aspire-based local orchestration.

## Evidence To Capture Once Code Exists

- vendor sign-up path works without billing
- configured branding appears in both the vendor PWA and the wallet pass
- a second device can scan the QR code and complete the join flow
- the Apple Wallet pass shows initial progress such as `0/5 coffees`
- awarding visits changes the displayed state to `1/5`, `2/5`, and onward
- the vendor PWA and wallet pass remain aligned after each visit award
