# FIC Platform Architecture Draft

This draft captures the current proposed architecture for FIC / Perk Up.

It intentionally mixes architecture and delivery-planning material while the product is still forming. After review, the architecture sections should remain in `docs/architecture/`, the decision points should be extracted into RFCs, and the phased delivery section should be decomposed into active plans and implementation specs.

## Current Founder Decisions

- Jo's business plan is the source of truth for business scope and product direction.
- The product must be cloud native.
- The platform should be architected to scale cleanly from zero without introducing premature operational complexity.
- Local development and test environments must run in Docker.
- Stage and production hosting target Azure.
- The application is a PWA, not a native mobile app.
- Apple Wallet support comes first.
- The core platform should be event-driven from the first meaningful slice.
- Document-oriented storage is preferred.
- Azure Cosmos DB is the current preferred primary datastore, subject to local-development validation.
- Wallet cards and the retailer-facing product must be white-label and configurable per retailer.
- The wallet card must show current customer progress state such as `2/5 coffees`.

## 1. Purpose

This document defines the engineering plan for the **FIC / Perk Up** platform described in the business plan: a privacy-first, wallet-based loyalty platform for independent coffee shops, paired with an automated loyalty marketing course. The core product goal is to let customers join instantly via QR code, add a loyalty card to Apple Wallet or Google Wallet, and participate in a loyalty programme without installing an app or providing personal data. Retailers should receive simple behavioural insight with minimal operational overhead.

The engineering plan is designed around these principles:

- **Cloud native, low operations**
- **Contract first** for external and internal integration boundaries
- **DDD** with explicit bounded contexts
- **Event-driven** core with projections
- **Privacy-first by default**
- **Lean, scalable, and inexpensive to run**
- **Simple enough for a two-person business to operate**
- **White-label by design**

---

## 2. Product Scope

### In scope for MVP

- Retailer onboarding and subscription activation
- Programme creation for a single loyalty programme per café
- QR-based customer join flow
- Wallet pass generation and update
- White-label retailer branding and configurable card presentation
- Anonymous visit / stamp tracking
- Reward unlock and redemption
- Retailer dashboard with simple KPIs
- Automated insight summaries
- Billing hooks for monthly subscription
- Support for a marketing site and course funnel living outside the core product

### First internal MVP/demo slice

The first slice should be an internal demo that proves the end-to-end product loop without billing complexity.

- vendor sign-up without billing
- upload brand assets or brand-guideline inputs
- configure a loyalty card such as `buy 5 coffees get 1 free`
- generate a QR code for customer join
- scan that QR code on another device and add the pass to Apple Wallet
- show customer progress on the wallet pass and in the vendor PWA
- award a visit internally and refresh wallet state from `0/5` to `1/5`, `2/5`, and so on

### Explicitly out of scope for MVP

- EPOS integrations
- Multi-programme logic per retailer
- CRM automation
- SMS / email customer marketing journeys
- Live support tooling
- Deep BI reporting
- Full microservice sprawl
- Real-time web socket UX

This keeps the platform aligned to the stated business plan goal of being simple, affordable, privacy-first and easy for independent cafés to adopt.

---

## 3. Architectural Style

## 3.1 Recommended style

A **DDD modular monolith with asynchronous workers** is the recommended starting point.

That means:

- one primary deployable for the web product and API
- one or more background workers for projections and wallet updates
- strict bounded contexts in code
- event-driven boundaries internally
- easy path to split out services later if scale or organisational complexity requires it

This approach gives the benefits of DDD and cloud-native architecture without introducing the operational burden of full microservices too early.

## 3.2 Why not full microservices now

Full microservices would add:

- more deployments
- more monitoring
- more failure modes
- more infrastructure cost
- more cognitive overhead

That conflicts with the lean operating model and the desire to keep ongoing effort very low. The business plan explicitly targets gradual scale, lean infrastructure and a modest infrastructure budget.

---

## 4. High-Level Cloud Architecture

## 4.1 Primary Azure components

### Web and API
- **Azure App Service**
- Hosts the retailer portal, public QR/join pages, wallet endpoints, and REST API
- ASP.NET Core application
- Serves the PWA / web UI and API from one deployable initially
- This is the intended stage and production hosting path unless a later RFC changes it

### Background processing
- **Azure Functions** or **Container Apps jobs / worker**
- Handles Cosmos DB change feed projections
- Handles wallet update jobs
- Handles scheduled summaries / housekeeping
- SignalR-backed live dashboard fan-out can also be triggered from worker-side projection updates when needed

### Data
- **Azure Cosmos DB**
- Event store and read-model store
- Low operational overhead
- Good fit for append-only events, customer-scoped aggregates and projections
- Preferred default because the product shape is document-heavy and Azure-native

### Cache / low-latency guardrail
- **Azure Cache for Redis**
- RU shielding for hot reads
- idempotency keys
- cooldowns / rate limiting
- optional hot cache for customer progress and programme config

### Object storage
- **Azure Storage Account**
- QR / pass assets if needed
- exports
- operational artefacts
- optional cold archive later

### Secrets and certificates
- **Azure Key Vault**
- Apple Wallet certificate material
- Google Wallet credentials
- Stripe keys
- app secrets

### Messaging
- **Service Bus** (optional but recommended)
- decouples wallet update and other async work from the hot path
- can be introduced from MVP if clean separation is desired

### Observability
- **Application Insights + Log Analytics**
- structured logging
- traces
- alerts

### Billing
- **Stripe** for subscription payments and billing webhooks
- Sage remains accounting / finance of record if required by the business operations plan.

## 4.2 External business systems

### Marketing / course site
- **WordPress** remains the marketing website and course delivery entry point as described in the business plan.
- WordPress should integrate with the platform through a narrow contract:
  - course enrolment handoff
  - retailer sign-up handoff
  - authenticated upgrade / subscribe CTA into the product

This separation keeps marketing tooling flexible while the product remains a dedicated cloud-native application.

## 4.3 Local development orchestration

Use Docker for local dependencies and runtime packaging.

Use .NET Aspire for local orchestration, service discovery, configuration wiring, and local observability during development.

Recommended local topology:

- Blazor web app
- background worker
- Cosmos DB emulator container
- Redis container
- Azurite when object storage behavior is needed
- Aspire dashboard for traces, logs, and metrics during development

---

## 5. DDD Bounded Contexts

The domain should be separated into the following bounded contexts.

## 5.1 Loyalty Core

Owns the business rules for:

- programme definition
- configurable reward item definition
- configurable number of visits or items required before reward
- stamp / progress accumulation
- reward unlocks
- reward redemption
- anti-abuse rules
- customer lifecycle in the loyalty system

Core domain concepts:

- `Retailer`
- `Programme`
- `BrandProfile`
- `CustomerCard`
- `Progress`
- `Reward`
- `Visit`
- `Redemption`

This is the strategic core.

## 5.2 Wallet Passes

Owns:

- Apple Wallet pass generation
- Google Wallet equivalent generation
- pass template rendering
- pass update orchestration
- pass registration identifiers and metadata
- branded stamp-state presentation on the pass
- pass field mapping from retailer configuration

This context consumes projections from Loyalty Core. It does not own loyalty truth.

## 5.2.1 White-label card configuration

Wallet cards and retailer-facing product surfaces must be configurable per retailer.

Minimum configurable elements for MVP:

- retailer name
- logo and brand colours
- reward item label
- number of visits or items required before reward
- stamp or progress presentation
- terms and short reward copy

The same retailer configuration should drive both the operator-facing PWA and the wallet pass presentation so branding and programme rules stay aligned.

## 5.2.2 Wallet card state

The wallet pass must display live customer progress state derived from the loyalty projection.

Minimum state shown on the pass for MVP:

- current progress such as `2/5 coffees`
- reward status such as `locked`, `unlocked`, or `redeemed`
- retailer branding
- reward description

The vendor PWA and the wallet pass should read the same progress projection so a vendor never sees different state than the customer.

## 5.3 Billing and Plans

Owns:

- subscription state
- plan entitlements
- usage cap rules
- billing webhook handling
- grace periods and upgrade prompts

## 5.4 Identity and Consent

Default mode is anonymous. However, the architecture should support an optional future identity-enabled tier.

Owns:

- optional customer email / phone
- consent records
- marketing preference state
- delete / export workflows

PII must be isolated from behavioural data.

## 5.5 Reporting and Insights

Owns:

- daily and weekly retailer metrics
- reward completion metrics
- customer activity summaries
- simple AI-generated insight summaries

This context is projection-first and must never query the event stream for live dashboard reads.

## 5.6 Admin / Support

Internal operational tooling only, kept minimal.

---

## 6. Contract-First Design

Contract-first means external behaviour is defined before implementation.

## 6.1 Public API contracts

Use **OpenAPI** for REST contracts.

Contracts to define first:

- retailer registration / authentication
- programme creation and retrieval
- retailer branding and wallet-card configuration
- QR join token generation
- customer join
- visit award
- reward redeem
- retailer dashboard queries
- billing webhook endpoint
- wallet pass endpoints

These contracts should be versioned and treated as product artefacts.

## 6.2 Internal integration contracts

Use explicit message contracts for asynchronous work.

Examples:

- `CustomerJoined`
- `VisitAwarded`
- `RewardUnlocked`
- `RewardRedeemed`
- `WalletPassUpdateRequested`
- `RetailerUsageThresholdReached`
- `SubscriptionActivated`

These should be defined as versioned contracts in a shared contracts package.

## 6.3 Website / course handoff contracts

The marketing site and email funnel should not directly couple to the product database.

Define handoff contracts such as:

- `StartTrial`
- `CreateRetailerFromCourseLead`
- `ResumeSubscriptionSignup`

This allows WordPress / marketing tooling to be changed later without rewriting the loyalty platform.

---

## 7. Event-Driven Domain Model

## 7.1 Why event-driven

The business plan is fundamentally based on tracking anonymous repeat behaviour, reward progression and lifecycle insight. An event-driven model fits naturally because loyalty actions are facts that happen over time.

## 7.2 Core domain events

Recommended minimum set:

- `CustomerJoined`
- `ProgrammeConfigured`
- `VisitAwarded`
- `RewardUnlocked`
- `RewardRedeemed`
- `AdjustmentApplied`
- `VisitReversed`

These are business events, not infrastructure events.

The first slice should prove a small but complete event chain rather than a broad event catalogue.

## 7.3 Event source of truth

The platform should use an append-only event stream in Cosmos DB as the behavioural source of truth, with projections for read performance.

This provides:

- auditability
- replayability
- flexibility if programme rules evolve later
- clean support for insight generation

---

## 8. Data Model and Storage Strategy

## 8.1 Cosmos DB containers

### `events`
Purpose:
- append-only event stream for loyalty behaviour

Suggested partition key:
- `tenantId|customerId`

Stores:
- domain events only

### `customer-progress`
Purpose:
- current progress for fast reads

Suggested partition key:
- `tenantId|customerId`

Stores:
- current stamp count
- current target count
- display-ready progress text such as `2/5 coffees`
- reward availability
- last activity date
- wallet identifiers
- current state for card rendering

### `retailer-stats`
Purpose:
- daily / weekly KPIs

Suggested partition key:
- `tenantId`

Stores:
- members count
- repeat visit rate
- reward completion rate
- rewards redeemed
- summary metrics for dashboard

### `programme-config`
Purpose:
- retailer programme settings

Suggested partition key:
- `tenantId`

Stores:
- threshold, reward text, branding, QR settings, soft caps

### `identity` (future / optional tier)
Purpose:
- optional customer PII and consent

Suggested partition key:
- `tenantId|customerId`

Stores:
- email, phone, consent flags, consent timestamps

This container must be isolated from the behavioural containers.

## 8.4 Local development note

Local development should use the Azure Cosmos DB Linux emulator in Docker where possible so the repo can stay Docker-first end to end.

Current caveat:

- the Linux-based emulator is still preview-only for NoSQL
- it runs in Docker, which fits the local workflow goal
- it only supports a subset of Cosmos DB features, so production-only capabilities must be avoided or explicitly tested in Azure before adoption

If emulator limitations become a drag on developer speed, the fallback should be a small shared Azure development environment rather than abandoning the document-store choice accidentally.

## 8.2 Why projections are mandatory

Dashboards, wallet rendering and retailer insight reads must use projections rather than querying the event stream directly. This protects RU costs and keeps latency predictable.

## 8.3 Redis usage

Redis should be used narrowly for:

- hot cache of `customer-progress`
- programme config cache
- idempotency keys for scans / awards
- cooldown and rate limiting rules

Redis is included for ultra-low latency and RU protection, not as a source of truth.

---

## 9. Privacy and Identity Strategy

## 9.1 Anonymous by default

The default product mode should collect no personal customer data, in line with the business plan and product positioning.

The platform should work with:

- anonymous customer ID
- loyalty events
- retailer and programme context
- behavioural analytics only

## 9.2 Optional identity-enabled mode

If later introduced, it must:

- be a separate tier or product capability
- keep PII in a separate container
- keep consent state explicit
- avoid polluting the behavioural model

## 9.3 Deletion and export

For the anonymous product, deletion needs are light.

If identity is enabled later, implement:

- export by `tenantId|customerId`
- delete PII independently
- optional anonymisation of linked behavioural records if required

---

## 10. Customer Lifecycle and Technical Flow

The business plan defines a QR-led lifecycle from discovery to reward redemption.

## 10.1 Join flow

1. Retailer creates a programme and receives a QR code
2. Customer scans QR
3. Public join page opens
4. Customer taps **Add to Wallet**
5. Wallet pass is created
6. `CustomerJoined` event is written
7. `customer-progress` projection is created
8. Apple Wallet pass issuance is the first supported wallet path for MVP

Precondition:

- the retailer's programme and branding configuration must already exist

## 10.2 Visit flow

Preferred MVP flow:

- customer presents wallet pass QR
- retailer scans using a simple scanner flow
- backend validates request
- `VisitAwarded` event is written
- projection updates current progress
- wallet update job is queued

Alternative receipt QR flow can be supported later if needed, because it is referenced in the business plan as an optional non-integrated path.

## 10.3 Reward flow

1. Visit threshold reached
2. `RewardUnlocked` event emitted
3. pass updated to show unlocked reward
4. retailer redeems reward
5. `RewardRedeemed` event emitted
6. progress resets or rolls over based on programme rules

## 10.4 Internal demo progress flow

1. Vendor creates a programme with branding and threshold
2. Customer joins from a separate device via QR
3. Wallet pass is issued showing initial state such as `0/5 coffees`
4. Vendor awards a visit from the internal demo interface
5. `VisitAwarded` event is written
6. progress projection updates to `1/5 coffees`, then `2/5 coffees`, and so on
7. wallet update is triggered so the pass reflects the same state

---

## 11. Application Topology

## 11.1 Initial deployment topology

### App Service: `platform-web`
Hosts:
- retailer portal
- public join pages
- wallet endpoints
- REST API

### Function App / Worker: `platform-workers`
Runs:
- Cosmos change feed processors
- wallet update handlers
- scheduled summaries
- billing / housekeeping jobs

### Cosmos DB account
Containers listed above

### Redis cache
Low-latency cache and idempotency store

### Key Vault
All secrets and certificates

### Storage account
Static artefacts and exports

This is cloud native, low maintenance and appropriate for the cost assumptions in the business plan.

---

## 12. API and UI Strategy

## 12.1 PWA / web approach

The customer-facing and retailer-facing product should be web-first.

Recommended stack:

- ASP.NET Core
- a PWA shell for retailer and customer-facing web experiences
- Blazor for retailer portal where helpful
- Razor Pages or lightweight web pages for public QR join and wallet add flow

Reasoning:

- retailer portal benefits from richer component model
- public customer flow must stay fast and simple
- one application can host both concerns initially
- PWA delivery keeps install friction low while preserving a mobile-friendly experience

The retailer-facing PWA should expose configuration of programme rules and branded wallet-card presentation as a first-class capability, not an afterthought.

## 12.2 Wallet priority

Wallet support should be phased:

- Apple Wallet first for the MVP
- Google Wallet after the Apple pass lifecycle, update model, and retailer operations are proven

## 12.3 No separate API service initially

There should be one logical API surface, but not necessarily a separate deployable service. The modular monolith should expose:

- `/api/*`
- `/join/*`
- `/wallet/*`
- `/portal/*`

This keeps deployment and operations simple.

## 12.4 Realtime and push model

Use SignalR for connected retailer-side experiences only:

- live dashboard refresh
- immediate feedback after awarding a visit
- operator notifications inside the vendor PWA

Do not use SignalR as the primary customer notification channel.

Apple Wallet pass updates should use the standard pass-update lifecycle:

- platform state changes
- wallet update work is queued
- Apple push notification is sent for the affected pass
- device requests the updated pass from the platform

Browser push notifications for the PWA are optional later work, not part of the first internal MVP/demo.

---

## 13. Anti-Abuse and Operational Integrity

Fraud risk is low value but still worth basic controls.

Minimum controls:

- idempotency key per award request
- cooldown windows per customer
- optional per-device / per-location limits
- audit trail through events
- admin adjustments through explicit `AdjustmentApplied`

Redis is the right place for transient anti-abuse controls.

---

## 14. Billing and Usage Caps

The business model is low monthly subscription, initially £15–£20 per café.

Engineering approach:

- Stripe owns payment collection
- Product owns entitlement state
- Billing bounded context manages plan logic

If usage caps are introduced, use a **soft cap with grace buffer**:

- notify retailer at threshold warning level
- allow small overage buffer
- stop new joins if cap remains exceeded after grace
- never break existing wallet users

This keeps upgrade behaviour automated and low-support.

---

## 15. Reporting and Insight Strategy

The business plan calls for simple retailer insight, not enterprise BI.

MVP dashboard should include:

- total loyalty members
- visits this week
- rewards redeemed
- average return gap
- reward completion rate

Insight summaries can be generated from projections, for example:

- busiest loyalty day
- average time to reward
- percentage of customers returning within 7 days

Do not build a complex report writer.

## 15.1 Metrics and KPI model

Separate platform telemetry from product KPIs.

Operational telemetry:

- request latency
- error rate
- projection lag
- wallet issuance latency
- wallet update latency
- Cosmos RU consumption
- Redis cache hit rate
- queue depth
- SignalR connection count

Business KPIs:

- vendor sign-up completion rate
- programme configuration completion rate
- QR-to-wallet conversion rate
- active wallet cards
- visits per member
- rewards unlocked
- rewards redeemed
- repeat visit rate
- average visits to reward

Business KPIs should come from domain events and projections, not from request logs.

## 15.2 Observability stack

Use OpenTelemetry as the application instrumentation baseline.

Environment approach:

- local: Aspire dashboard for logs, traces, and metrics
- stage and production: Azure Monitor and Application Insights

If live portal KPIs are pushed to connected vendors in real time, they should be served from projections and cached views, not by querying the event stream directly.

---

## 16. Engineering Repo and Solution Structure

Suggested .NET solution structure:

```text
src/
  Platform.Web/
  Platform.Workers/
  Contracts/
  BuildingBlocks/
  LoyaltyCore/
    Domain/
    Application/
    Infrastructure/
  WalletPasses/
    Domain/
    Application/
    Infrastructure/
  Billing/
    Domain/
    Application/
    Infrastructure/
  Identity/
    Domain/
    Application/
    Infrastructure/
  Reporting/
    Domain/
    Application/
    Infrastructure/
  Tests/
```

Rules:

- no UI code inside domain projects
- application layer orchestrates use cases
- infrastructure implements ports / adapters
- contracts are versioned and shared
- each slice should add validation scripts or automated tests alongside implementation

---

## 17. Delivery Plan

## Phase 0 - Foundation

- confirm contracts
- establish bounded contexts
- prove local Docker development with the chosen datastore and wallet tooling
- provision Azure resources
- configure CI/CD
- define OpenAPI and message contracts
- define the validation harness for per-slice scripts and acceptance checks

## Phase 1 - Core MVP

- retailer sign-up
- create programme
- configure white-label branding and reward rules
- customer join QR flow
- wallet pass generation
- visible wallet progress state such as `2/5 coffees`
- visit award and reward redeem
- event store and projections
- basic dashboard

## Phase 2 - Operational Hardening

- Redis idempotency and rate limits
- usage cap logic
- Stripe billing integration
- monitoring and alerts
- backup / key rotation / certificate lifecycle

## Phase 3 - Insight Layer

- automated summaries
- retailer trend metrics
- simple benchmark-ready projection model

## Phase 4 - Optional Identity Tier

- separate identity container
- explicit consent model
- export / delete flows
- tightly scoped communications capability if the business decides to offer it

---

## 18. Non-Functional Requirements

### Availability
- platform should tolerate transient worker failure without losing loyalty truth
- wallet updates are eventually consistent and non-blocking

### Performance
- award / redeem hot path should favour point reads and append writes
- Redis used to protect RU and maintain low latency

### Security
- secrets only via Key Vault
- least privilege between app and data stores
- PII isolated when enabled

### Cost control
- projection-first querying
- narrow indexing
- no cross-partition analytics in the hot path
- cache hot read models

### Maintainability
- modular monolith
- contract-first boundaries
- minimal deploy topology

---

## 19. Key Engineering Decisions

1. **Start as a DDD modular monolith, not full microservices**
2. **Start event-driven from the first meaningful slice**
3. **Use Cosmos DB as event and projection store**
4. **Use projections for all operational reads**
5. **Use Redis narrowly for latency, idempotency and RU protection**
6. **Serve API and web product from one primary deployable initially**
7. **Build the product as a PWA**
8. **Support Apple Wallet first**
9. **Treat white-label configuration as a core domain capability**
10. **Keep wallet progress state explicit and projection-backed**
11. **Keep customer mode anonymous by default**
12. **Keep identity isolated in a separate container if introduced later**
13. **Treat WordPress and course tooling as external to the core platform**
14. **Use contract-first design for APIs and integration events**
15. **Use Aspire for local orchestration and observability**
16. **Require validation at every delivery slice**
17. **Optimise for low operations and gradual scale**

---

## 20. Summary

This engineering plan supports the business plan by delivering a privacy-first, wallet-based loyalty product that is simple for independent coffee shops to adopt, inexpensive to run, and architecturally strong enough to evolve. It preserves the key product promises of no app, no required personal data, simple QR onboarding, wallet-based loyalty participation and retailer insight, while keeping infrastructure lean and maintainable.

The recommended path is:

- cloud-native Azure PaaS
- contract-first API and event design
- DDD modular monolith
- Cosmos event model with projections
- Redis for RU protection and hot path performance
- optional identity as a separate future capability

This should give the founders a strong technical foundation without overbuilding the product.
