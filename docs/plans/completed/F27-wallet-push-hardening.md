# F27 Wallet Push Hardening

## Goal
- Make Wallet pass refresh delivery predictable and diagnosable after merchant actions, so demo and support flows do not rely on guesswork.

## Scope
- Tighten the Wallet push dispatch path around APNs response handling and token-lifecycle hygiene.
- Keep pass-refresh status explicit in merchant and support surfaces when push is unavailable, skipped, retry-needed, or delivered.
- Add one focused troubleshooting lane for push readiness in the local Wallet runbook and support UI copy.
- Preserve the current issue-stamp-refresh founder demo flow while reducing hidden failure modes.

## Out Of Scope
- Non-Apple delivery channels.
- Full production alerting and observability rollout.
- Multi-staff merchant account workflows.

## Proof
- `scripts/validate-f27-wallet-push-hardening.sh`
