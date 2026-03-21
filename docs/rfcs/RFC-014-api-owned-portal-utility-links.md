# RFC-014 API-Owned Portal Utility Links

## Problem

Portal utility links are currently hardcoded in the frontend shell, which creates drift risk as utility IA evolves and duplicates navigation ownership.

## Decision

Expose utility links from the API portal navigation payload and have Next.js shell render those links from contract.

## Scope

- Extend shared portal navigation contracts with utility links.
- Generate utility links in C# portal navigation builder.
- Update Next.js `PortalShell` to accept contract utility links and render them.
- Preserve a static fallback list for resilience.
- Extend API tests to verify utility links are returned.

## Consequences

- Utility IA ownership shifts to backend/domain boundary.
- Frontend shell becomes simpler and less likely to drift from intended route topology.
