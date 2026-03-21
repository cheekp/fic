# F33 Acceptance

- `/api/v1/catalogue/shop-types` returns active shop type payloads.
- `/api/v1/merchants` can create a merchant payload.
- `/api/v1/session/complete-signup` configures credentials and signs in merchant session.
- `/api/v1/merchants/{merchantId}/workspace` requires session ownership and returns workspace for signed-in owner.
- `src/Fic.Platform.Frontend` contains a Next.js app with signup and workspace routes wired to `/api/v1`.
- `MerchantApiTests` pass in `tests/Fic.Platform.Web.Tests`.
