# F38 Roadmap Unified With Nav Contract

## Goal

Serve both portal rail and roadmap/tube-map progression from one API contract.

## Scope

- Extend `PortalNavigationContract` with roadmap payload.
- Emit roadmap in backend nav builder for signup/workspace flows.
- Consume roadmap in Next.js `OnboardingJourney` across signup/workspace routes.
- Validate via API tests + frontend build.

## Proof

- `dotnet test tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj`
- `npm run build` in `src/Fic.Platform.Frontend`
- `scripts/validate-f38-roadmap-unified-with-nav-contract.sh`
