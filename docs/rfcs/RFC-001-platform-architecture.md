# RFC-001 Platform Architecture

## Status
Proposed

## Context

FIC needs an architecture that can start small, stay low-operations, and preserve a clear path from business plan to shipped product. The first meaningful slice is an internal MVP/demo where a vendor can:

- sign up without billing
- upload branding inputs
- configure a loyalty card such as `buy 5 coffees get 1 free`
- generate a customer join QR code
- scan that QR code on another device
- add the card to Apple Wallet
- have the customer present the wallet pass back at point of sale
- scan the customer pass from the vendor website/PWA
- see progress state such as `2/5 coffees` on the wallet pass and in the vendor PWA

The repo also needs a stable operating model for spec-driven delivery, validation, and entropy control.

## Decision

Adopt the following baseline architecture.

### Application architecture

- C# and ASP.NET Core throughout
- Blazor Web App as the PWA shell
- modular monolith with explicit bounded contexts
- background worker for projections, wallet updates, and asynchronous processes

### Runtime topology

- local development in Docker
- .NET Aspire AppHost for local orchestration, service wiring, and observability
- Azure App Service for the primary web deployable in stage and production
- Azure Functions or a worker deployable for asynchronous workloads
- Redis and any external event broker stay optional behind feature flags

### Data and messaging

- Azure Cosmos DB as the preferred primary datastore
- event-driven domain model from the first meaningful slice
- projection-backed reads for wallet state, vendor views, and dashboard KPIs
- Redis for narrow concerns only: idempotency, cache, and hot-path protection
- start without a mandatory external broker if in-process events plus persisted projections are enough

### Product surfaces

- vendor-facing PWA
- public customer join flow served from the same web application
- customer experience is mobile web plus Wallet, not an installed app
- Apple Wallet first
- Google Wallet later, after Apple Wallet lifecycle is proven

### Realtime and notifications

- SignalR only for connected vendor-side realtime experiences
- Apple Wallet updates via the pass web-service lifecycle and Apple push notifications
- no general customer push-notification system in the first slice

### White-label and state

- white-label configuration is a core domain capability
- retailer branding and programme configuration drive both the vendor PWA and wallet pass rendering
- wallet progress state such as `2/5 coffees` is projection-backed domain state, not display-only text
- `MerchantAccounts` is a distinct bounded context for vendor onboarding and tenant setup
- setup catalogue options (shop types, card types, templates) are contract-backed configuration with active/inactive visibility flags, not hardcoded UI constants

### Delivery discipline

- Jo's business plan remains the business source of truth
- all meaningful slices require a spec and a validation path
- architecture changes are captured in RFCs before implementation-heavy work proceeds

## Consequences

### Positive

- one primary language and stack across UI, API, and worker code
- strong fit for a small team
- low operational overhead at launch
- clear separation between domain truth and realtime/UI concerns
- straightforward local developer experience with Docker and Aspire

### Negative

- Cosmos DB local emulation must be proven early because emulator limitations could affect inner-loop productivity
- Blazor plus Wallet integration will still require selective JavaScript interop for browser and platform-specific capabilities
- event-driven design adds some complexity even when kept intentionally small
- privacy review remains required before moving beyond internal MVP, even if the product is anonymous by design

## Open Questions

- Is the Cosmos DB emulator good enough for day-to-day development, or is a shared Azure dev environment needed?
- Should the first visit-award flow be vendor scan only, or should receipt QR be included immediately?
- What is the minimum acceptable brand-input model for the first vendor onboarding flow?
