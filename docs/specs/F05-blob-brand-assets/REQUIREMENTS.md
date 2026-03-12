# F05 Blob Brand Assets Requirements

## Goal

Move merchant brand assets from the local file-only implementation to a blob-backed storage path that works with Azurite locally and Azure Blob Storage in hosted environments, while preserving one public asset route for the portal, join flow, wallet preview, and Wallet package generation.

## In Scope

- blob-backed merchant brand asset store
- config-driven store selection between local file and blob-backed storage
- Azurite-ready AppHost wiring
- public brand asset endpoint backed by the active asset store
- Wallet package support for reading merchant branding through the asset store abstraction
- slice validation

## Out Of Scope

- merchant brand editing UI
- image resizing or optimization jobs
- CDN or cache invalidation
- production secret rotation

## Functional Requirements

- `F05-REQ-001` The repo must support a blob-backed merchant brand asset store implementation using Azure Blob Storage semantics.
- `F05-REQ-002` The active brand asset store must remain swappable by configuration so local file storage can still be used when blob storage is not enabled.
- `F05-REQ-003` The web app must serve merchant assets through one stable public request path regardless of the underlying storage implementation.
- `F05-REQ-004` Merchant signup, the vendor portal, the join flow, the wallet preview, and Apple Wallet package generation must all read merchant logo data through the same storage abstraction.
- `F05-REQ-005` The AppHost must support Azurite-based local orchestration for brand assets when blob mode is enabled.

## Storage Requirements

- `F05-STO-001` The blob store must write merchant logos into a dedicated container for merchant brand assets.
- `F05-STO-002` Merchant logos must continue to be limited to PNG uploads for this slice.
- `F05-STO-003` The public request path for brand assets must remain `/merchant-brand-assets/...`.

## Validation Requirements

- `F05-VAL-001` The slice must expose `./scripts/validate-f05-blob-brand-assets.sh`.
- `F05-VAL-002` Validation must confirm the blob-backed store, the AppHost Azurite wiring, and the public asset route exist.
- `F05-VAL-003` Validation must build the updated solution successfully.
