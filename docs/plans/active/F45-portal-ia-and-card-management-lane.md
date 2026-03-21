# F45 Portal IA And Card Management Lane

## Goal

Deliver a cleaner, hierarchy-first portal shell and a scalable customer card management lane so daily operations feel premium and predictable.

## Scope

- Add a reusable public portal header with burger-based site hierarchy and right-side auth actions.
- Rework merchant workspace composition to avoid competing navigation panes in core flow.
- Replace customers stacked-card view with table-first card management including visual card previews and actionable controls.
- Preserve existing API contracts and route URLs while improving UX composition.
- Add F45 validator and capture build/QA evidence.

## Proof

- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj --filter MerchantApiTests`
- `cd src/Fic.Platform.Frontend && npm run build`
- `scripts/validate-f45-portal-ia-and-card-management-lane.sh`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3301 npm run qa:signup-flow`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3302 npm run qa:workspace-slices`
