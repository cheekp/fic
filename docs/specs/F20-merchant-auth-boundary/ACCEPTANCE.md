# F20 Acceptance

- The billing/onboarding handoff lets a merchant set an account password before the workspace opens.
- The login page signs an existing merchant into their workspace with owner email and password.
- A merchant who is not signed in cannot use the merchant workspace route as the normal entry path.
- A signed-in merchant cannot open another merchant's workspace and receives an access-denied path instead.
- Logging out clears the session and routes the merchant back to the public product entry page.
- Merchant workspace behavior, Wallet routes, and existing tests continue to pass.
- Automated tests cover merchant login, workspace ownership, and logout behavior.
