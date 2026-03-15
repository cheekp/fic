# F23 Requirements

## Objective
- The selected programme workspace must make daily customer card management feel obvious, quick, and separate from programme configuration.

## Functional Requirements
- The selected programme workspace must expose `Customers` as a distinct subsection alongside `Operate`, `Configure`, and `Insights`.
- `Operate` must stay focused on:
  - customer join QR
  - till-side stamping
  - quick daily-use context
- `Customers` must show issued customer passes grouped by operational status.
- Customer card status must be explicit in the data presented to the UI.
- At minimum, customer card management must distinguish between:
  - active in progress
  - reward ready / unlocked
  - redeemed
  - inactive due to programme schedule window
- Customer card rows must make the key actions easy to find:
  - preview
  - wallet pass
  - load code for till-side stamping
  - redeem when eligible
- Programme configuration and customer card management must no longer compete on the same screen.

## Founder Demo Constraints
- Wallet passes remain the only live customer delivery output.
- Customer card statuses may be derived from the current programme window plus reward state instead of introducing a whole new state engine.
