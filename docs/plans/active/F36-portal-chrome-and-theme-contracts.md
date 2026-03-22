# F36 Portal Chrome And Theme Contracts

## Goal

Establish a reusable portal shell and contract layer in Next.js so signup/workspace routes share one coherent chrome system with mobile burger navigation and desktop rail consistency.

## Scope

- Add `PortalShell`, `PortalTopBar`, `PortalRail`, and `PortalRailDrawer` components.
- Add `PortalSurface` primitives for shared surface composition.
- Add frontend contracts:
  - `PortalNavItemContract`
  - `PortalThemeContract`
  - `ficPortalTheme` default
- Wire shell/chrome into:
  - `/portal/signup`
  - `/portal/signup/plan/[merchantId]`
  - `/portal/signup/billing/[merchantId]`
  - `/portal/merchant/[merchantId]`
- Preserve existing functional behavior and C# API workflows.

## Out Of Scope

- Full API-backed navigation contract response payload.
- Re-theming every historical route in one pass.
- Replacing existing onboarding roadmap logic.

## Proof

- `scripts/validate-f36-portal-chrome-and-theme-contracts.sh`
- `npm run build` in `src/Fic.Platform.Frontend`

## Phase 2 Status (Current Slice)

- Completed: shared nav contract adapter now builds route-slice badges/completion in one place (`lib/portal-contract-adapter.ts`).
- Completed: workspace rail state now derives from domain checklist and card counts (`buildWorkspacePortalNav` input uses `setupChecklist` + `cardsCount`).
- Completed: merchant-brand theme override now resolves with null-safe fallback to FIC defaults (`resolvePortalTheme`).
- Completed: shell nav reflects completion state consistently via adapter-provided flags.
- Completed: shared brand-token wiring now lets the Next.js shell and workspace surfaces consume one theme seam instead of scattering hardcoded palette values across pages.
