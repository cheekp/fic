# F37 API Nav Contract Source Of Truth

## Goal

Promote portal navigation state to an API contract so route badges, completion, and disabled states are built once in C# and consumed by Next.js.

## Scope

- Add `PortalNavigationContract` records to `Fic.Contracts`.
- Add API endpoints for signup/workspace nav contract retrieval.
- Add `PortalNavigationContractBuilder` in web services.
- Wire Next.js signup/workspace pages to API nav contract calls.
- Add/extend API tests for nav endpoints and auth behavior.

## Proof

- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj`
- `npm run build` in `src/Fic.Platform.Frontend`
- `scripts/validate-f37-api-nav-contract-source-of-truth.sh`
