# F23 Customer Card Management

## Goal
- Make issued customer cards easy to understand and operate by separating programme operations from customer card management and giving customer passes clear statuses.

## Scope
- Keep `Operate` focused on join QR and till-side stamping.
- Add a distinct `Customers` area inside the selected programme workspace.
- Group issued customer cards by operational status.
- Make customer card actions easier to scan and use.
- Keep programme-level insight and configuration separate from customer card management.

## Out Of Scope
- New delivery channels beyond the current Wallet-first path.
- Staff/team workflows.
- Production CRM or messaging.
- APNs hardening beyond the existing demo-ready baseline.

## Proof
- `scripts/validate-f23-customer-card-management.sh`
