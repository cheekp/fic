# F41 Workspace Next-Action Contract

## Goal

Deliver one backend-owned next-action contract so workspace onboarding renders a single clear immediate task lane.

## Scope

- Add `nextAction` payload shape to portal navigation contracts.
- Implement workspace next-action builder logic in C# from setup checklist state.
- Render workspace setup taskboard from `nextAction` in Next.js.
- Keep roadmap rendering and sequencing contract unchanged.
- Add API test assertions for workspace navigation `nextAction`.
- Add slice validator script and keep docs/rfc/spec in sync.

## Proof

- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj --filter MerchantApiTests`
- `cd src/Fic.Platform.Frontend && npm run build`
- `cd src/Fic.Platform.Frontend && npm run qa:signup-flow`
- `cd src/Fic.Platform.Frontend && npm run qa:workspace-slices`
- `scripts/validate-f41-workspace-next-action-contract.sh`
