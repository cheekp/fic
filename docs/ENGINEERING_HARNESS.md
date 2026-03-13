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
  - active or archived delivery planning
- `docs/specs/`
  - slice requirements and acceptance criteria
- `scripts/`
  - validator and workflow helpers tied to slices

## Working Loop

1. Start from business intent or a concrete founder need.
2. Update architecture or RFC material if the slice changes product or system shape.
3. Write or revise the slice spec in `docs/specs/<slice>/`.
4. Implement the minimum code needed for the slice.
5. Add or update a repo validator script for that slice.
6. Prove the slice with the validator before handing it off.
7. Update harness or runbooks if the working method has changed.

## Invariants

- Specs come before substantial implementation.
- Every meaningful slice gets explicit acceptance criteria.
- Every meaningful slice gets a scriptable validator where feasible.
- Docs and code change together when behavior changes materially.
- White-label branding is core product behavior, not optional decoration.
- Wallet card state is domain state, not view-only text.
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

Each new slice should follow that same pattern: spec, code, validator, evidence.

## Current Product Reality

The repo now supports:

- self-serve merchant signup
- merchant-owned workspace flow after signup
- collapsible `FIC` platform shell around the merchant workspace
- merchant reward configuration
- merchant brand editing inside the workspace
- loyalty card template editing inside the workspace
- issued customer card management separated from template editing
- PNG logo upload for merchant branding
- blob/local brand asset storage seam
- join QR generation
- customer join flow
- wallet preview flow
- Apple Wallet `.pkpass` issuance seam
- merchant-scoped brand theming across workspace, join flow, and wallet preview
- platform-level nav reserved for home, merchant creation, and future account concerns such as billing/auth

The repo does not yet represent finished production behavior for:

- full merchant auth and tenant isolation
- live Apple pass update web service and APNs loop
- billing
- production privacy/legal review
- merchant self-service brand editing after onboarding
- production-grade workspace/auth model
- multi-programme or multi-card-template merchant support

## Local Proof Loop

Current high-value local loops:

- validator-first:
  - run the slice validator before claiming a slice is done
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

## Review Triggers

Review the harness when:

- the slice workflow changes
- a new delivery seam is introduced
- local proof and demo steps change
- the repo starts carrying stale “current state” guidance

## Immediate Next Focus

The next likely product slices are:

- real Apple Wallet signed-pass demo completion with founder-friendly setup
- pass update lifecycle after `VisitAwarded`
- stronger merchant account and tenant ownership boundaries
- production auth/session flow that removes the remaining demo assumptions
