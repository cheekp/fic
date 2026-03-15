# F22 Programme Model And Templates

## Goal
- Strengthen the product model so a programme is clearly a shop-owned loyalty object with a type, a starter template, and a current customer delivery output, instead of reading like a synonym for a wallet card.

## Scope
- Make programme type, starter template, and customer delivery explicit in the model.
- Keep `Shop -> Programmes -> Selected Programme` as the workspace hierarchy.
- Update the first-programme and new-programme flow so template choice feels explicit and future-proof.
- Make programme configuration read as `programme setup + current customer delivery`, not just card editing.
- Keep the Wallet-first path as the live customer delivery while leaving room for future outputs.

## Out Of Scope
- New delivery channels beyond the current Apple Wallet path.
- Full customer card management overhaul.
- New execution engines beyond the current Wallet-first baseline.
- Production billing or staff/team access.

## Proof
- `scripts/validate-f22-programme-model-and-templates.sh`
