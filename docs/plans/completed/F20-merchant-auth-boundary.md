# F20 Merchant Auth Boundary

- Completed via PR `#21`.
- Outcome:
  - merchant onboarding now captures an owner password before the workspace opens
  - the app establishes a cookie-backed merchant session after onboarding or login
  - merchant workspace routes now enforce signed-in ownership and redirect to login or access denied when appropriate
