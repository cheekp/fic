# F35 Next.js Premium PWA UI Note

## Context

The initial Next.js migration slices proved API parity but still carried placeholder UI scaffolding. We now need a production-facing interaction system that feels premium, mobile-first, and aligned with the existing FIC/merchant brand direction.

## Decision

- Adopt Tailwind CSS as the Next.js styling foundation with restrained luxury design tokens.
- Introduce shadcn-style component primitives and Radix-based interaction controls.
- Add PWA essentials (manifest + service worker registration + offline fallback route).
- Keep C# backend APIs and Blazor routes unchanged so current demo and validation workflows remain intact.

## Consequences

- Next frontend can iterate quickly with a coherent, reusable UI system.
- Mobile ergonomics and visual polish improve without introducing an incompatible brand language.
- Blazor delivery remains operational during migration, reducing transition risk.
