# F22 Requirements

## Objective
- The workspace must treat a programme as the merchant-facing loyalty object, with the starter template and current customer delivery output clearly represented but not confused for the programme itself.

## Functional Requirements
- A programme must expose:
  - a programme type
  - a starter template
  - a current customer delivery type
  - a current customer output label
- The programme creation flow must make those concerns explicit without overwhelming the merchant:
  - template chosen first
  - programme type visible
  - current customer delivery visible
- The product must continue to support at least two starter templates:
  - a visit reward programme
  - a coffee-plus-food offer programme
- The selected programme workspace must present the programme as the root object and the current customer delivery output as a subordinate concern.
- Programme configuration must separate:
  - the programme rule and schedule
  - the current customer delivery/output
- The Wallet-first founder demo path must remain intact after the model update.

## Founder Demo Constraints
- Apple Wallet may remain the only live customer delivery implementation.
- The coffee-plus-food offer may continue to use the current programme engine as long as the merchant-facing flow and copy remain explicit about the offer shape.
