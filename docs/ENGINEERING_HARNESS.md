# Engineering Harness

This document is the repo-level operating harness for FIC.

Its job is to keep the business plan, architecture, specs, code, and validation loop aligned as we move quickly. The harness is not the architecture itself and it is not a backlog. It defines how we make changes without letting the repo drift.

## Purpose

- keep business intent, product promise, and implementation slices connected
- stop entropy from building up across docs, code, and demos
- force each meaningful slice to be spec-backed and validator-backed
- keep the team fast without losing architectural discipline

## Source Of Truth

- `docs/business/fic-business-plan-v1.docx`
  - source of truth for business intent, product positioning, and commercial direction
- `docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md`
  - current technical architecture direction
- `docs/rfcs/`
  - decisions that need explicit commitment or revision history
- `docs/specs/`
  - implementation-ready slice contracts

If engineering intent conflicts with the business plan, the conflict must be raised explicitly and resolved in docs before code silently sets a new direction.

## Current Stack Direction

- cloud-native deployment target
- Docker/Aspire for local orchestration and test loops
- Azure for stage and production
- ASP.NET Core Blazor Web App for the merchant-facing application shell
- Apple Wallet first for customer pass delivery
- Cosmos DB for product data direction
- Blob storage for merchant brand assets

## Document Layers

- `docs/business/`
  - source business material
- `docs/ENGINEERING_HARNESS.md`
  - repo-level workflow, layering, and entropy rules
- `docs/architecture/`
  - architecture drafts and system notes
- `docs/runbooks/`
  - founder-demo and operator workflows
- `docs/rfcs/`
  - decision proposals and records
- `docs/plans/`
  - delivery status and sequencing
  - this is where `active`, `planned`, and `completed` live
- `docs/specs/`
  - slice requirements and acceptance criteria
  - specs are not a status board
- `scripts/`
  - validator and workflow helpers tied to slices

## Working Loop

1. Start from business intent or a concrete founder need.
2. Update architecture or RFC material if the slice changes product or system shape.
3. Make sure the slice is represented in `docs/plans/` with the right status.
4. Write or revise the slice spec in `docs/specs/<slice>/`.
5. Implement the minimum code needed for the slice.
6. Add or update a repo validator script for that slice.
7. Add targeted automated tests for domain/state-heavy behavior when the slice introduces meaningful logic.
8. Prove the slice with the validator before handing it off.
9. Move the plan record when the slice status changes, and update harness or runbooks if the working method has changed.

## Plan Vs Spec

- `plans` own workflow status
  - use `active/`, `planned/`, and `completed/` there
  - if you prefer the word `open`, treat current `planned/` as that concept unless we do a repo-wide rename
- `specs` own implementation detail for a slice
  - one slice folder per implemented or implementation-ready slice
  - do not add `active/`, `open/`, or `completed/` folders under `docs/specs/`
- a spec can stay in place after the slice lands
  - it becomes the historical contract for what was built
  - status still belongs to the corresponding plan record, PR, and merge state

## Invariants

- Specs come before substantial implementation.
- Every meaningful slice gets explicit acceptance criteria.
- Every meaningful slice gets a scriptable validator where feasible.
- Docs and code change together when behavior changes materially.
- White-label branding is core product behavior, not optional decoration.
- Wallet card state is domain state, not view-only text.
- Programme is the merchant-facing unit of loyalty operation; the loyalty card is a programme output, not a peer workspace root.
- Merchant assets are stored as assets, not buried inside primary documents.
- Public join flow, merchant operations, and wallet delivery stay behind clear seams even inside a modular monolith.

## Slice Discipline

Completed slice chain to date:

- `F01`
  - internal MVP demo flow
  - validator: `scripts/validate-f01-internal-mvp-demo.sh`
- `F02`
  - Apple Wallet pass issuance seam
  - validator: `scripts/validate-f02-apple-wallet-pass.sh`
- `F03`
  - wallet demo readiness and founder runbook
  - validator: `scripts/validate-f03-wallet-demo-readiness.sh`
- `F04`
  - merchant brand asset persistence baseline
  - validator: `scripts/validate-f04-merchant-brand-assets.sh`
- `F05`
  - blob-backed merchant brand assets
  - validator: `scripts/validate-f05-blob-brand-assets.sh`
- `F06`
  - merchant brand engine
  - validator: `scripts/validate-f06-merchant-brand-engine.sh`
- `F07`
  - merchant-first workspace information architecture
  - validator: `scripts/validate-f07-merchant-workspace-ia.sh`
- `F08`
  - workspace polish baseline plus bUnit coverage
  - validator: `scripts/validate-f08-workspace-polish-and-bunit.sh`
- `F09`
  - merchant workspace hierarchy correction
  - validator: `scripts/validate-f09-merchant-workspace-polish.sh`
- `F10`
  - programme-centric workspace navigation
  - validator: `scripts/validate-f10-programme-workspace-nav.sh`
- `F11`
  - programme workspace polish and daily-use reduction of visual density
  - validator: `scripts/validate-f11-programme-workspace-polish.sh`
- `F12`
  - company brand surfaces and support-layer polish
  - validator: `scripts/validate-f12-company-brand-surfaces.sh`
- `F13`
  - entry-flow cleanup so the home page and signup path stay product-led and low-friction
  - validator: `scripts/validate-f13-entry-flow-polish.sh`
- `F14`
  - further reduction of narrative and competing signals in the public entry lane
  - validator: `scripts/validate-f14-entry-lane-focus.sh`
- `F15`
  - programme workspace focus so the merchant area reads as a tool rather than an explanation
  - validator: `scripts/validate-f15-programme-workspace-focus.sh`
- `F16`
  - selected-programme worksurface polish so operate and configure feel lighter and less stacked
  - validator: `scripts/validate-f16-programme-worksurface-polish.sh`

Each new slice should follow that same pattern: spec, code, validator, evidence.

## Current Active Slice

- `F17`
  - merchant workspace visual polish so the programme rail and work pane feel calmer, tighter, and less dashboard-like
  - validator: `scripts/validate-f17-workspace-visual-polish.sh`

## Current Product Reality

The repo now supports:

- self-serve merchant signup
- thin merchant signup that hands off quickly into the merchant workspace
- merchant-owned workspace flow after signup
- `FIC` shell focused on acquisition and signup, with a lighter merchant-owned workspace frame after onboarding
- merchant reward configuration
- merchant brand editing inside the workspace
- shop settings available as an in-context merchant workspace surface
- multiple programmes per merchant shop
- programmes live under a merchant shop context
- the programme workspace now uses nested `Operate`, `Configure`, and `Insights` sections
- loyalty cards are configured within a programme rather than acting as a peer merchant root concept
- issued customer card management separated from template editing
- PNG logo upload for merchant branding
- blob/local brand asset storage seam
- join QR generation
- customer join flow
- wallet preview flow
- Apple Wallet `.pkpass` issuance seam
- merchant-scoped brand theming across workspace, join flow, and wallet preview
- platform-level concerns reduced to lightweight workspace utilities once inside a merchant-owned area
- company-layer home, account, billing, and consultancy surfaces separated from merchant-owned workspace theming
- product entry flow now separates company backing from the primary FIC product CTA
- public entry now needs ongoing restraint so company positioning never overwhelms the product lane
- programme workspace is organised under shop -> programmes, with one selected programme work surface at a time
- shop settings now need to stay lightweight and secondary to daily programme operations
- selected-programme operate/configure surfaces should keep shrinking toward day-to-day utility rather than explanatory UI
- workspace presentation now needs ongoing restraint so the programme rail reads like navigation and the right side reads like one focused tool surface

The repo does not yet represent finished production behavior for:

- full merchant auth and tenant isolation
- live Apple pass update web service and APNs loop
- billing
- production privacy/legal review
- production-grade workspace/auth model

## Local Proof Loop

Current high-value local loops:

- validator-first:
  - run the slice validator before claiming a slice is done
- targeted test loop:
  - run the focused xUnit suite for state-heavy behavior before trusting UI-only changes
- component test loop:
  - use bUnit for merchant-workspace interaction rules where UI behavior is meaningful but a full browser test is still too heavy
- direct web preview:
  - run the Blazor app with the local profile for browser and LAN demos
- wallet demo:
  - use `docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md`
  - use `scripts/run-wallet-demo-lan.sh` when Apple signing material is configured

## Entropy Rules

- do not let README, harness, runbooks, and slice docs drift apart
- remove or revise stale “next step” language when a slice lands
- avoid duplicating architecture decisions inside specs
- avoid burying business assumptions inside code comments or UI copy
- prefer one current doc per concern over multiple partially-correct versions
- keep only one active plan at a time unless we intentionally decide to run stacked slices
- avoid creating a spec until the slice is concrete enough to build in the current horizon
- if a spec becomes abandoned before implementation, either delete it or fold the intent back into architecture or plans
- before starting a new slice, prune stale placeholders, old screenshots, and validator text that no longer describe the product truth

## Review Triggers

Review the harness when:

- the slice workflow changes
- a new delivery seam is introduced
- local proof and demo steps change
- the repo starts carrying stale “current state” guidance

## Immediate Next Focus

The next likely product slices are:

- company brand surfaces and support-layer polish
- entry-flow cleanup so the home page and signup path stay product-led and low-friction
- further reduction of narrative and competing signals in the public entry lane
- merchant workspace focus so programmes feel like a rail plus one working surface
- selected-programme work surface polish so operate and configure feel calmer and more direct
- merchant workspace visual polish so the rail, context bar, and work pane feel less crowded
- real Apple Wallet signed-pass demo completion with founder-friendly setup
- pass update lifecycle after `VisitAwarded`
- stronger merchant account and tenant ownership boundaries
- production auth/session flow that removes the remaining demo assumptions
