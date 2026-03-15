# F21 Requirements

## Objective
- The product must guide a merchant through one clear lane: create the shop, confirm billing/account access, then create the first programme and its current customer delivery output.

## Functional Requirements
- Shop signup must create the merchant account and brand baseline only; it must not silently create a starter programme.
- Billing/account setup must remain the second onboarding step and must sign the merchant into their own workspace on completion.
- A merchant with no programmes must land in a first-programme setup experience instead of an empty or misleading workspace.
- The merchant workspace must model `Shop` as the root context and `Programmes` as a child operating area inside the shop.
- Programme creation must be explicit about the chosen template and the current customer delivery output.
- The product must offer at least two starter programme templates in the first-programme setup flow:
  - a visit reward starter
  - a coffee-plus-food offer starter
- The selected programme workspace must make clear that the loyalty card is the current delivery output of the programme, not the programme itself.
- Programme operations such as join QR, stamping, issued passes, and programme-specific insight must remain scoped to the selected programme.
- Shop-level settings and shop-level insight must remain separate from programme configuration and daily customer operations.

## Founder Demo Constraints
- The current Wallet-first delivery path may remain the only live delivery implementation.
- The second starter template may reuse the current programme engine as long as the merchant-facing flow and copy make the offer shape explicit.
- Existing Wallet issuance, Wallet update lifecycle, and merchant auth boundaries must continue to work after the IA reset.
