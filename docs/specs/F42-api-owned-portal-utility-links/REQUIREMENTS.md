# F42 Requirements

## Functional

1. `PortalNavigationContract` includes utility link contracts.
2. Signup and workspace portal navigation endpoints return utility link lists.
3. `PortalShell` renders utility links from contract when provided.
4. `PortalShell` preserves fallback utility links if contract links are absent.
5. Existing utility destinations remain reachable (blogs, training, consultancy, account, billing, logout).
6. API tests assert utility links are present in portal navigation responses.

## Non-Functional

1. No regression in onboarding/workspace workflow routes.
2. Utility links remain readable and keyboard navigable in desktop header.
