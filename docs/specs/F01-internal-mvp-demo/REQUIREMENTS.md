# F01 Internal MVP Demo Requirements

## Goal

Prove the first end-to-end internal demo for FIC: vendor setup, white-label loyalty-card configuration, QR-based customer join, Apple Wallet pass issuance, and visible progress state.

## Scope

In scope:

- vendor sign-up without billing
- vendor brand input and basic brand asset upload
- loyalty-programme configuration for one programme per vendor
- QR code generation for customer join
- customer join from a second device
- Apple Wallet pass issuance
- vendor website/PWA scan flow for in-store visit capture
- vendor-visible and customer-visible progress state such as `2/5 coffees`
- visit-award path that updates the wallet pass and vendor PWA state

Out of scope:

- billing and subscriptions
- Google Wallet
- advanced analytics
- CRM or marketing automation
- EPOS integrations

## Functional Requirements

- `F01-REQ-001` A vendor can create an account and access the internal demo without billing.
- `F01-REQ-001A` Vendor onboarding is owned by a merchant-account flow, not buried inside loyalty-programme setup.
- `F01-REQ-002` A vendor can configure one loyalty programme with:
  - reward item label
  - reward threshold such as `5`
  - short reward copy
- `F01-REQ-003` A vendor can provide white-label branding inputs for the PWA and wallet card, including:
  - retailer display name
  - logo
  - primary brand colour
  - secondary or accent colour
- `F01-REQ-004` The system generates a join QR code for the configured programme.
- `F01-REQ-005` A customer using a separate device can scan the QR code and reach the join flow.
- `F01-REQ-006` The join flow can issue an Apple Wallet pass for the configured programme.
- `F01-REQ-007` The wallet pass must display vendor branding and current customer progress state.
- `F01-REQ-008` Progress state must be represented explicitly as numerator and denominator, for example `2/5 coffees`.
- `F01-REQ-009` The vendor PWA must display the same progress state as the wallet pass for the same customer.
- `F01-REQ-010` The system provides a vendor scan flow where the customer presents the wallet pass and the vendor scans it from the vendor website/PWA.
- `F01-REQ-011` Awarding a visit writes a business event and updates the customer progress projection.
- `F01-REQ-012` After a visit is awarded, the wallet pass can be refreshed to show the updated progress state.
- `F01-REQ-013` The platform must preserve one source of truth for programme configuration so wallet rendering and vendor views do not drift.
- `F01-REQ-014` The slice must expose basic operational telemetry for join, wallet issuance, visit award, and projection update paths.
- `F01-REQ-015` The customer-facing join flow must remain browser-based and must not require an installed app.
- `F01-REQ-016` Redis and external event transport are not mandatory for F01 and should remain optional implementation choices.

## Domain Requirements

- `F01-DOM-001` `ProgrammeConfigured`, `CustomerJoined`, and `VisitAwarded` are minimum business events for this slice.
- `F01-DOM-001A` `MerchantAccountCreated` is the minimum merchant-onboarding event for this slice.
- `F01-DOM-002` Customer progress is projection-backed and must include:
  - current count
  - target count
  - reward state
  - wallet-pass display fields
- `F01-DOM-003` White-label configuration is part of domain configuration, not a separate front-end theme object.
- `F01-DOM-004` Merchant onboarding belongs to a distinct merchant-account boundary even if implemented inside the same deployable.

## Validation Requirements

- `F01-VAL-001` The slice must have a scriptable validation entry point at `./scripts/validate-f01-internal-mvp-demo.sh`.
- `F01-VAL-002` Validation must confirm the presence of the RFC, requirements, acceptance, and contract docs for the slice.
- `F01-VAL-003` When code exists, the same validation entry point should be extended to run automated checks for the join, issue-pass, and visit-award path.
