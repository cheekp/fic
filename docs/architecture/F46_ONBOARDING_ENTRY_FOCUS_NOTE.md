# F46 Onboarding Entry Focus Note

## Intent

Make signup entry feel intentionally guided by reducing top-chrome distractions in onboarding routes while preserving full portal hierarchy on public and workspace surfaces.

## Decisions

- Introduce an onboarding-focused header mode for `PortalShell`.
- Keep signup, plan, and billing routes in a focused shell that prioritizes conversion and completion over utility navigation.
- Preserve full utility/header navigation for workspace and public browsing contexts.

## Outcome

- Cleaner mental model at the exact point users start setup.
- Lower chance of route escape or context switching during onboarding.
- Better alignment between landing promise and signup execution flow.
