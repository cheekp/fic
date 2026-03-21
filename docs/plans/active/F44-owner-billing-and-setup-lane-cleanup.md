# F44 Owner/Billing and Setup Lane Cleanup

## Goal

Make onboarding feel fast and obvious by tightening owner/billing progression and simplifying post-billing workspace setup UI.

## Scope

- Add compact owner/billing sub-step indicator in signup billing route.
- Tighten stage framing copy and reduce redundant helper text.
- Replace workspace onboarding taskboard stack with a single next-action card.
- Keep setup blade interaction for shop details/logo and route handoff intact.
- Add validator for F44 docs/code presence.

## Proof

- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj --filter MerchantApiTests`
- `cd src/Fic.Platform.Frontend && npm run build`
- `scripts/validate-f44-owner-billing-and-setup-lane-cleanup.sh`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3301 npm run qa:signup-flow`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3302 npm run qa:workspace-slices`
