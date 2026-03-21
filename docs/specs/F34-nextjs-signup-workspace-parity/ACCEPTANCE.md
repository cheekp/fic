# F34 Acceptance

- `src/Fic.Platform.Frontend/app/portal/signup/plan/[merchantId]/page.tsx` exists and routes to billing.
- `src/Fic.Platform.Frontend/app/portal/signup/billing/[merchantId]/page.tsx` completes signup credentials through `/api/v1/session/complete-signup`.
- `src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx` supports operate/configure/customers slices.
- Workspace slice can create a programme from a template, update programme settings, award visits, and redeem rewards via `/api/v1`.
- `tests/Fic.Platform.Web.Tests/MerchantApiTests.cs` covers programme create/update and customer reward action flow.
- `scripts/validate-f34-nextjs-signup-workspace-parity.sh` passes.
