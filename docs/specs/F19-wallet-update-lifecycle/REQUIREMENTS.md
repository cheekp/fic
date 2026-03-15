# F19 Requirements

## Objective
- The product must support the Apple Wallet pass update lifecycle after a pass has been issued.

## Functional Requirements
- A signed Wallet pass must include `authenticationToken` and `webServiceURL`.
- The app must expose Apple Wallet web-service endpoints for:
  - pass registration
  - pass unregistration
  - updated serial lookup
  - updated pass download
  - Wallet log intake
- The server must keep in-memory demo registrations for Wallet-enabled passes.
- The server must validate Wallet authentication tokens before allowing registration or updated pass retrieval.
- When a merchant awards a visit or redeems a reward, the changed pass must be visible through the Wallet web-service update path immediately.
- Updated pass retrieval must return the latest `.pkpass` archive for the changed serial number.
- Preview fallback behavior must remain intact when Apple Wallet signing is not configured.

## Founder Demo Constraints
- The update path may stay within the current in-memory demo state.
- The product should support local LAN founder demos.
- APNs delivery may remain a follow-on concern as long as the web-service update contract is implemented and tested end to end.
