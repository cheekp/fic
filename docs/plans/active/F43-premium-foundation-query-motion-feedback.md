# F43 Premium Foundation Query/Motion/Feedback

## Goal

Ship a premium interaction foundation that improves clarity, responsiveness, and consistency without changing core API/domain behavior.

## Scope

- Add TanStack Query provider in Next app root.
- Add Sonner toaster host and apply to key signup/workspace mutations.
- Add Vaul-powered mobile utility drawer in portal shell.
- Add subtle Framer Motion content transition in shell.
- Introduce reusable query hooks for portal navigation and workspace reads.
- Add slice validator and docs updates.

## Proof

- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj --filter MerchantApiTests`
- `cd src/Fic.Platform.Frontend && npm run build`
- `scripts/validate-f43-premium-foundation-query-motion-feedback.sh`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3301 npm run qa:signup-flow`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3302 npm run qa:workspace-slices`
