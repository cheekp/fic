# F04 Merchant Brand Assets Acceptance

## Acceptance Checks

1. Run `./scripts/validate-f04-merchant-brand-assets.sh`.
2. Create a merchant with a PNG logo upload.
3. Confirm the merchant workspace, join page, wallet preview, and Wallet pass all use the same stored logo reference.

## Acceptance Criteria

- `F04-ACC-001` The repo contains F04 requirements and acceptance docs for merchant brand assets.
- `F04-ACC-002` Merchant branding is persisted through a storage abstraction rather than only through transient form state.
- `F04-ACC-003` The app serves uploaded merchant assets from a stable public path.
- `F04-ACC-004` Wallet pass generation supports stored merchant PNG assets.
- `F04-ACC-005` The validation script builds successfully.

## Evidence To Capture

- a PNG logo uploaded during merchant signup is visible in the merchant workspace
- the same logo appears on the join page
- the same logo appears on the wallet preview
- the same logo is included in the generated Wallet pass package
