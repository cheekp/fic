# F05 Blob Brand Assets Acceptance

## Acceptance Checks

1. Run `./scripts/validate-f05-blob-brand-assets.sh`.
2. Review the AppHost storage wiring for Azurite-backed local runs.
3. Confirm merchant assets are served through `/merchant-brand-assets/...` irrespective of store mode.

## Acceptance Criteria

- `F05-ACC-001` The repo contains F05 requirements and acceptance docs.
- `F05-ACC-002` The web app includes a blob-backed merchant brand asset store.
- `F05-ACC-003` The asset-store abstraction supports both save and read paths.
- `F05-ACC-004` AppHost includes Azurite-ready blob storage orchestration when the feature is enabled.
- `F05-ACC-005` Wallet package generation reads merchant brand assets through the storage abstraction rather than direct filesystem assumptions.
- `F05-ACC-006` The validation script builds successfully.
