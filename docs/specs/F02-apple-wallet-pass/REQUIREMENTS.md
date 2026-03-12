# F02 Apple Wallet Pass Requirements

## Goal

Replace the wallet preview-only join outcome with a real Apple Wallet `.pkpass` download path when signing material is configured, while preserving a clean preview fallback for local and incomplete environments.

## Scope

In scope:

- Apple Wallet `.pkpass` package generation
- wallet-pass signing seam and configuration model
- join-flow behavior that downloads a real pass when signing is configured
- preview fallback when signing is not configured
- minimum Apple Wallet field mapping for branding, progress, and scan code
- pass download endpoint in the web app

Out of scope:

- Apple push notification certificates and APNs delivery
- pass-update web service endpoints
- Google Wallet
- production secret distribution
- final merchant image-processing pipeline

## Functional Requirements

- `F02-REQ-001` The join flow must support a real Apple Wallet `.pkpass` response for a configured merchant card.
- `F02-REQ-002` When Apple Wallet signing is not configured, the join flow must fall back to the existing wallet preview path instead of failing silently.
- `F02-REQ-003` The join surface must clearly route the customer either to a real pass download or to preview mode based on runtime capability.
- `F02-REQ-004` The web application must expose a pass-download endpoint for an issued customer card.
- `F02-REQ-005` The pass package must use the same merchant branding and progress state as the vendor workspace and preview card.
- `F02-REQ-006` The pass must carry the customer scan code as the machine-readable barcode message.
- `F02-REQ-007` The pass must show visible progress state such as `2/5 coffees`.
- `F02-REQ-008` The first real pass style must be valid for a loyalty programme and should use an Apple Wallet card style suited to merchant loyalty.
- `F02-REQ-009` The implementation must keep Apple signing configuration outside code and bind it from configuration.
- `F02-REQ-010` The implementation must preserve the existing `Wallet:AppleWalletSigningConfigured` capability switch or equivalent runtime guard.

## Domain Requirements

- `F02-DOM-001` The wallet pass serial number must be stable per customer card and derived from the platform’s wallet-card identity.
- `F02-DOM-002` The wallet pass barcode message must match the vendor scan path input for the same customer card.
- `F02-DOM-003` Wallet package generation must consume shared projection-backed state rather than rebuild progress from unrelated UI data.
- `F02-DOM-004` Apple Wallet issuance belongs to the `WalletPasses` bounded context and must remain isolated from merchant onboarding and loyalty progression rules.

## Configuration Requirements

- `F02-CONF-001` Minimum Apple Wallet signing configuration must include:
  - pass type identifier
  - team identifier
  - organization name
  - pass description
  - PKCS#12 certificate path
  - PKCS#12 certificate password
  - Apple WWDR certificate path
- `F02-CONF-002` A missing or incomplete configuration must not crash the merchant join flow; it must trigger preview fallback.

## Validation Requirements

- `F02-VAL-001` The slice must have a scriptable validation entry point at `./scripts/validate-f02-apple-wallet-pass.sh`.
- `F02-VAL-002` Validation must confirm the presence of the F02 requirements, acceptance, and contract docs.
- `F02-VAL-003` Validation must confirm the existence of wallet-pass service code, download endpoint wiring, and preview fallback behavior.
