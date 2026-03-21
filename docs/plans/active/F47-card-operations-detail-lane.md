# F47 Card Operations Detail Lane

## Goal

Improve customer-card operations quality by adding drill-in detail and stronger action context in the customers route.

## Scope

- Add status filter controls to customer cards lane.
- Add deterministic sort for large card sets.
- Add card detail dialog with preview, status, progress, metadata, and recent timeline activity.
- Keep redeem/copy actions accessible in both list and detail surfaces.
- Add F47 validator and run full proof loop.

## Proof

- `scripts/validate-f47-card-operations-detail-lane.sh`
- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj --filter MerchantApiTests`
- `cd src/Fic.Platform.Frontend && npm run build`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3301 npm run qa:signup-flow`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3302 npm run qa:workspace-slices`
