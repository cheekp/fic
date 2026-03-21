# F42 API-Owned Portal Utility Links

## Goal

Ensure portal utility navigation is contract-driven from API instead of hardcoded in frontend shell.

## Scope

- Add utility link list to shared portal navigation contract.
- Build utility links in C# portal nav builder.
- Render utility links from API contract in Next.js `PortalShell`.
- Keep resilient fallback link list in shell for API failure/absence.
- Add API assertions and slice validator.

## Proof

- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj --filter MerchantApiTests`
- `cd src/Fic.Platform.Frontend && npm run build`
- `cd src/Fic.Platform.Frontend && npm run qa:signup-flow`
- `cd src/Fic.Platform.Frontend && npm run qa:workspace-slices`
- `scripts/validate-f42-api-owned-portal-utility-links.sh`
