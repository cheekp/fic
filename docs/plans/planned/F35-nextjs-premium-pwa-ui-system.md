# F35 Next.js Premium PWA UI System

## Goal

Elevate the Next.js migration surface to a premium, mobile-first PWA while keeping the current C# API and Blazor workflows stable.

## Scope

- Add Tailwind, postcss, and design-tokenized global styles.
- Add shadcn-style UI primitives and Radix-based interaction components.
- Upgrade signup/plan/billing/workspace pages to use the new UI system.
- Add PWA manifest, service worker registration, icon assets, and offline route.
- Prove both Next build and key backend tests still pass.

## Out Of Scope

- Deleting Blazor routes.
- Full design parity with every historical Blazor CSS rule.

## Proof

- `scripts/validate-f35-nextjs-premium-pwa-ui-system.sh`
