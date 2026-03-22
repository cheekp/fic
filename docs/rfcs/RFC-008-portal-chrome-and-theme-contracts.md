# RFC-008 Portal Chrome And Theme Contracts

## Problem

Portal pages in Next.js are converging visually but still compose shell/chrome concerns ad hoc per route. This increases drift risk and slows parity with the Blazor shell quality.

## Decision

Adopt a reusable portal chrome system and explicit contracts:

- `PortalShell` + `PortalRail` + `PortalTopBar` + mobile `PortalRailDrawer`
- `PortalSurface` primitives for repeatable hero/metric/card patterns
- `PortalNavItemContract` and `PortalThemeContract` in frontend types
- FIC default theme with optional merchant overrides and safe fallback behavior
- Theme tokens are the single frontend seam for platform-default and merchant-derived branding; route components should consume the shared theme contract rather than hardcode palette values locally.

The first implementation slice wires signup/plan/billing/workspace routes.

## Consequences

- Shared navigation and chrome semantics across portal slices.
- Better DDD alignment via explicit contracts instead of hardcoded route chrome.
- Lower UI entropy and simpler iteration under design guardrails.
