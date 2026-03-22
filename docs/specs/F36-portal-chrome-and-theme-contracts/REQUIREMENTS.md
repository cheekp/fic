# F36 Requirements

## Objective

Introduce a reusable Next.js portal chrome system with explicit theme/navigation contracts to reduce drift and support future multi-app portal reuse.

## Functional Requirements

- `PortalShell` must exist and provide:
  - top bar region
  - rail navigation region
  - primary content frame
  - optional secondary panel slot
- Mobile must expose rail navigation via burger-triggered drawer.
- Desktop must render persistent rail navigation without burger dependency.
- Portal nav entries must be defined via typed contract objects, not page-local hardcoded JSX fragments.
- Theme tokens must be represented by a typed contract with FIC defaults and safe fallbacks.
- The portal theme contract must carry compiled presentation tokens beyond primary/accent, including canvas, surface, line, button, glow, and logo-plate values needed to keep the Next.js shell aligned with the backend merchant brand engine.
- Signup, plan, billing, and workspace routes must render within `PortalShell`.

## Operational Requirements

- Existing route behavior and API calls must remain functional.
- `npm run build` in `src/Fic.Platform.Frontend` must pass.
- Design changes must remain compatible with `docs/runbooks/NEXTJS_DESIGN_PASS_GUARDRAILS.md`.
