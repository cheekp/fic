# F03 Wallet Demo Readiness Acceptance

## Acceptance Checks

1. Run `./scripts/validate-f03-wallet-demo-readiness.sh`.
2. Review [APPLE_WALLET_LOCAL_DEMO.md](/Users/paulcheek/dev/fic/docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md) for the full founder demo path.
3. Confirm the merchant workspace shows either `Signed Apple Wallet ready` or `Preview fallback`.

## Acceptance Criteria

- `F03-ACC-001` The repo contains a wallet demo-readiness spec and acceptance record.
- `F03-ACC-002` The repo contains an Apple Wallet local demo runbook.
- `F03-ACC-003` The web project supports `.NET` user secrets for local Wallet signing configuration.
- `F03-ACC-004` The merchant workspace surfaces the current Wallet capability state.
- `F03-ACC-005` The repo contains a LAN demo run script.
- `F03-ACC-006` The validation script builds successfully.

## Demo Evidence To Capture

- an iPhone scans the merchant QR and reaches the join flow
- `Add to Apple Wallet` downloads a signed `.pkpass`
- Wallet offers the native add flow
- the merchant workspace exposes a direct pass-download action for joined cards
