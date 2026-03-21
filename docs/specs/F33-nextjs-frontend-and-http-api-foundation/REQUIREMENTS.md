# F33 Requirements

## Objective

Create a migration-ready frontend seam by serving product behavior through versioned HTTP APIs and proving a Next.js consumer flow.

## Functional Requirements

- Backend must expose versioned JSON endpoints under `/api/v1` for:
  - shop/catalogue lookup
  - merchant creation
  - session current/login/complete-signup/logout
  - merchant workspace retrieval
- Merchant-scoped workspace/programme/brand API operations must enforce merchant session ownership.
- Next.js frontend must exist in `src/Fic.Platform.Frontend` and call `/api/v1`.
- Next.js must provide working routes for:
  - `/`
  - `/portal/signup`
  - `/portal/merchant/[merchantId]`

## Operational Requirements

- Blazor app and existing non-API endpoints must continue compiling.
- New API contracts must be integration-tested with `WebApplicationFactory`.
