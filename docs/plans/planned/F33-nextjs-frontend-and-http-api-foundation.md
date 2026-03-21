# F33 Next.js Frontend And HTTP API Foundation

## Goal

Open a safe migration lane from Blazor to Next.js by introducing a stable HTTP API boundary and a working Next.js onboarding/workspace starter flow.

## Scope

- Add `/api/v1` endpoints in `Fic.Platform.Web` for:
  - catalogue lookups
  - merchant creation
  - session bootstrap (current, complete-signup, logout)
  - merchant workspace read
  - programme and brand mutations
- Create `src/Fic.Platform.Frontend` Next.js app with:
  - home page
  - merchant signup page
  - merchant workspace read page
- Keep existing Blazor routes and behavior unchanged for non-migrated surfaces.
- Add backend integration tests for key API session/workspace paths.

## Out Of Scope

- Full route parity for all existing Blazor pages.
- Production deployment changes for Next.js hosting.
- Retiring Blazor components in this slice.

## Proof

- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj --filter "FullyQualifiedName~MerchantApiTests"`
