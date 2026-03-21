# F48 Card Lifecycle Bulk Operations

## Goal

Enable explicit card lifecycle management (suspend/reactivate/archive) and operator bulk actions in the customers route.

## Scope

- Add lifecycle transitions in C# platform state and card snapshots.
- Add authenticated lifecycle APIs for single and bulk actions.
- Add customers lane multi-select + bulk action UX and status filter coverage.
- Add slice validator and Merchant API tests for lifecycle transitions.

## Proof

- `scripts/validate-f48-card-lifecycle-bulk-ops.sh`
- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj --filter MerchantApiTests`
- `cd src/Fic.Platform.Frontend && npm run build`
