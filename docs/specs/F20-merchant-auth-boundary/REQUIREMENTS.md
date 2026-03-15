# F20 Requirements

## Objective
- The product must move from shared demo access into a merchant-owned session boundary for the merchant workspace.

## Functional Requirements
- Merchant onboarding must collect or confirm an account password before the merchant enters the workspace.
- Merchant credentials must be stored in a verifiable form rather than as plain text.
- The login route must authenticate a merchant with their owner email and password.
- The product must establish a cookie-backed merchant session after login or onboarding completion.
- Merchant-owned workspace routes must require an authenticated merchant session.
- A signed-in merchant must only be able to access their own merchant workspace.
- Logging out must clear the merchant session and return the user to the public entry lane.
- Existing Wallet pass, join, and merchant operations behavior must remain intact once a merchant is signed in.

## Founder Demo Constraints
- The baseline may remain in-memory for merchant records and credentials.
- Password recovery may remain a stub as long as the user is routed to the right support surface.
- The internal demo workspace index may remain available only as an explicitly non-standard development tool.
