# F03 Wallet Demo Readiness Requirements

## Goal

Make the Apple Wallet pass slice operationally demoable from a founder laptop to a real iPhone without committing certificate material into the repo.

## In Scope

- local secure configuration path for Apple Wallet signing
- LAN demo run workflow
- wallet capability visibility in the merchant workspace
- direct wallet-pass download action for existing cards
- default Wallet pass image assets suitable for local demo use
- validation script for the slice

## Out Of Scope

- APNs push and Wallet web-service registration
- background wallet refresh jobs
- Azure Key Vault integration
- Google Wallet

## Functional Requirements

- `F03-REQ-001` The web app must support local secret-based configuration for Apple Wallet signing without requiring tracked config edits.
- `F03-REQ-002` The repo must provide a runbook for the end-to-end local iPhone demo flow.
- `F03-REQ-003` The repo must provide a single LAN-friendly run command or script for the local demo host.
- `F03-REQ-004` The merchant workspace must clearly indicate whether the current environment will issue a signed Apple Wallet pass or fall back to preview mode.
- `F03-REQ-005` The merchant workspace must expose a direct wallet-pass download action for an existing customer card when signing is configured.
- `F03-REQ-006` The local demo flow must preserve the existing preview fallback behavior when signing is incomplete.
- `F03-REQ-007` Default image assets for the Wallet package must be PNG files present in the web app so the pass package does not rely on a missing or ad hoc icon source.

## Configuration Requirements

- `F03-CONF-001` The web app project must declare a stable `.NET` user-secrets identifier for local Apple Wallet demo configuration.
- `F03-CONF-002` The runbook must document both user-secrets keys and environment-variable equivalents.
- `F03-CONF-003` Certificate material such as `.p12` and `.cer` files must remain git-ignored.

## Validation Requirements

- `F03-VAL-001` The slice must expose `./scripts/validate-f03-wallet-demo-readiness.sh`.
- `F03-VAL-002` Validation must confirm the runbook, run script, wallet workspace capability cues, and user-secrets support exist.
- `F03-VAL-003` Validation must build the web app and wallet service code path without external code changes.
