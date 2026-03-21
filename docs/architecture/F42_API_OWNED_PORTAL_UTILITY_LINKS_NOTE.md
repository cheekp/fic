# F42 API-Owned Portal Utility Links Note

## Intent

Move portal utility navigation (blogs, training, consultancy, account, billing, logout) out of hardcoded frontend lists and into the API contract.

## Decisions

- Add utility link contracts to `PortalNavigationContract`.
- Build utility links in C# inside `PortalNavigationContractBuilder` for signup and workspace surfaces.
- Keep labels/hrefs as data in API responses so frontend shell remains presentation-only.
- Preserve safe Next.js fallback links for resilience if the API payload is unavailable.

## Outcome

- One source of truth for portal utility IA.
- Safer expansion of portal destinations without duplicating nav logic in frontend pages/components.
