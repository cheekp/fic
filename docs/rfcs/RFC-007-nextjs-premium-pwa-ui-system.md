# RFC-007 Next.js Premium PWA UI System

## Problem

Route parity alone is not enough; current Next screens lack the quality bar required for premium merchant use on mobile.

## Decision

Adopt a mobile-first premium PWA frontend system in Next.js using:

- TypeScript + Tailwind CSS
- shadcn-style reusable UI components
- Radix primitives for interactive controls
- subtle motion and strong typography
- restrained palette aligned to existing FIC merchant branding

while preserving the existing C# API boundary and Blazor workflow.

## Consequences

- Consistent, testable frontend primitives for all subsequent route migrations.
- Better installability and offline resilience via PWA shell behavior.
- No disruption to the existing Blazor app path while migration continues.
