# F46 Onboarding Entry Focus

## Goal

Strengthen landing-to-signup cohesion by making setup routes visually focused and less navigationally noisy.

## Scope

- Add onboarding header mode to shared portal shell.
- Use onboarding mode across signup, plan, and billing routes.
- Preserve existing route contracts, roadmap behavior, and workspace shell behavior.
- Add F46 validator and capture standard proof loop.

## Proof

- `scripts/validate-f46-onboarding-entry-focus.sh`
- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj --filter MerchantApiTests`
- `cd src/Fic.Platform.Frontend && npm run build`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3301 npm run qa:signup-flow`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3302 npm run qa:workspace-slices`
