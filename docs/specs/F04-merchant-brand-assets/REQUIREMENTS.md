# F04 Merchant Brand Assets Requirements

## Goal

Replace transient in-memory logo handling with a persisted merchant brand asset path so the vendor portal, join flow, wallet preview, and Apple Wallet package can all render from the same merchant-owned branding source.

## In Scope

- merchant logo upload persistence
- merchant brand asset storage seam
- local file-backed asset storage for development
- public asset delivery path for portal, join, preview, and pass generation
- Wallet package support for stored merchant PNG assets
- validation for the slice

## Out Of Scope

- Azure Blob Storage implementation
- Azurite orchestration in AppHost
- image resizing pipelines
- multiple brand assets per merchant
- merchant self-service brand editing after signup

## Functional Requirements

- `F04-REQ-001` Merchant signup must support a persisted logo upload path rather than relying only on in-memory data URIs.
- `F04-REQ-002` Uploaded logos must be validated as PNG files for the MVP brand path.
- `F04-REQ-003` The application must expose a brand asset storage abstraction so local storage can be replaced by Azure Blob Storage later without changing merchant onboarding or Wallet pass code.
- `F04-REQ-004` The local development implementation must store uploaded merchant assets outside tracked source files.
- `F04-REQ-005` The vendor workspace, customer join flow, wallet preview, and Wallet package generation must all read the same persisted merchant logo reference.
- `F04-REQ-006` If no PNG logo is uploaded, the application may continue to use the existing fallback visual treatment for browser-based surfaces.
- `F04-REQ-007` Apple Wallet pass generation must support loading merchant PNG logo bytes from the persisted asset path when available.

## Storage Requirements

- `F04-STO-001` The first implementation must use a local file-backed asset store rooted in `App_Data/merchant-brand-assets` or equivalent non-tracked storage.
- `F04-STO-002` Public merchant asset URLs must be served from a stable request path such as `/merchant-brand-assets/...`.
- `F04-STO-003` The asset store interface must be compatible with a future Azure Blob Storage or Azurite-backed implementation.

## Validation Requirements

- `F04-VAL-001` The slice must expose `./scripts/validate-f04-merchant-brand-assets.sh`.
- `F04-VAL-002` Validation must confirm the presence of the merchant brand asset store seam, the local asset store implementation, the public asset path wiring, and the Wallet package support for stored assets.
- `F04-VAL-003` Validation must build the web app and wallet service path successfully.
