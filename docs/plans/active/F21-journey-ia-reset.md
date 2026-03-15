# F21 Journey IA Reset

## Goal
- Reset the onboarding and merchant workspace flow so the product reads naturally as `shop -> programmes -> programme delivery`, without silently creating a loyalty programme during shop signup.

## Scope
- Make shop signup create the merchant account only.
- Keep billing/account setup as a separate second step.
- Land the merchant in a first-programme setup flow after billing instead of a pre-created starter programme.
- Make `Programmes` the clear operating area inside a shop.
- Make the selected programme clearly separate from its current customer delivery output.
- Add starter programme templates so creation is explicit rather than vague.

## Out of Scope
- Production-grade billing.
- Staff/team account support.
- Fully new programme execution engines beyond the current Wallet-first baseline.
- Non-Wallet customer delivery implementations beyond template-ready groundwork.

## Proof
- `scripts/validate-f21-journey-ia-reset.sh`
