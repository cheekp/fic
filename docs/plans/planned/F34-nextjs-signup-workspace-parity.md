# F34 Next.js Signup And Workspace Parity

## Goal

Deliver Next.js parity for merchant signup plan/billing and workspace operate/configure/customers slices using the existing `/api/v1` seam.

## Scope

- Add Next.js routes:
  - `/portal/signup/plan/[merchantId]`
  - `/portal/signup/billing/[merchantId]`
  - `/portal/merchant/[merchantId]` (operate/configure/customers)
- Expand frontend API client usage for:
  - workspace reads by selected programme
  - programme create/update
  - award visit and redeem reward
  - template lookup
- Expand integration tests for API behaviours used by these slices.

## Out Of Scope

- Final visual parity with every Blazor style token.
- Deleting Blazor components.
- Production deployment topology changes.

## Proof

- `scripts/validate-f34-nextjs-signup-workspace-parity.sh`
