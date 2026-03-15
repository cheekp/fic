# F24 Wallet Demo Critical Path

## Goal
- Make the founder demo feel reliable end to end: create a programme, publish join, add a real Wallet pass, stamp a visit, and request a visible Wallet refresh without the merchant guessing what happened.

## Scope
- Keep Wallet pass issuance and Wallet pass refresh as distinct but connected capabilities.
- Add a Wallet refresh notifier seam that requests pass updates for registered devices after stamp and redeem actions.
- Surface Wallet readiness more clearly in company support and merchant workflow surfaces.
- Tighten the merchant-side messaging around what happened after a pass-affecting action.
- Keep the Wallet-first founder demo runbook current with the implementation.

## Out Of Scope
- Multi-staff operations.
- Non-Apple customer delivery channels.
- Production-grade APNs credential rotation or key management.
- Broader UX polish outside the Wallet demo critical path.

## Proof
- `scripts/validate-f24-wallet-demo-critical-path.sh`
