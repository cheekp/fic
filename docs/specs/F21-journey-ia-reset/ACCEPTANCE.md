# F21 Acceptance

- A merchant can complete shop signup and billing without a programme being silently created during the signup step.
- Completing billing lands the merchant in a first-programme setup flow inside their own workspace.
- The workspace makes `Shop` the root context and `Programmes` the child operating area.
- A merchant with no programmes sees a clear first-programme setup experience rather than programme operations for a non-existent programme.
- Programme creation offers at least two starter templates and makes the current customer delivery output explicit.
- The selected programme screen continues to support daily-use actions such as join QR, stamping, issued customer passes, and programme-specific insight.
- Shop settings and shop insight remain available without competing with programme operations.
- Existing auth, Wallet issuance, and Wallet refresh tests continue to pass.
- Automated tests cover the no-programme onboarding state and first-programme creation flow.
