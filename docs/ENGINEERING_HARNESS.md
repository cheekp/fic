# Engineering Harness

This document is the repo-level operating harness for FIC.

Its job is to keep architecture, delivery, and repository hygiene coherent as the product grows. The harness sits above RFCs, plans, and specs. It is not a backlog and it is not the product architecture itself.

## Purpose
- keep the repo as the system of record for product and engineering decisions
- reduce entropy as the startup moves quickly
- enforce a clear path from business intent to shipped code
- make a small team operationally effective without overbuilding process

## Source Of Truth
- `docs/business/fic-business-plan-v1.docx` is the source of truth for business intent, product promise, and commercial direction.
- Architecture, RFCs, plans, and specs must refine the business plan, not silently override it.
- If a technical design conflicts with the business plan, raise the conflict explicitly and resolve it in an RFC.

## Document Layers
- `docs/business/` contains source business inputs
- `docs/architecture/` contains technical architecture drafts and reviewed architecture notes
- `docs/rfcs/` contains decision proposals and decision records
- `docs/plans/active/` contains the single current delivery plan
- `docs/plans/planned/` contains sequenced but inactive plans
- `docs/plans/completed/` contains closed records kept for traceability
- `docs/specs/` contains implementation-ready requirements and acceptance
- `scripts/` contains validation and slice-proof commands

## Flow Of Work
1. Business or founder input changes.
2. Architecture is drafted or revised in `docs/architecture/`.
3. Decisions that need agreement are captured in an RFC.
4. The chosen decision is broken into an active plan.
5. The active plan is decomposed into buildable specs.
6. Code and tests are implemented against the active spec.
7. Completed work is moved out of the active path.

## Invariants
- One active delivery plan at a time.
- Specs must map to a reviewed architecture note or RFC.
- Acceptance criteria exist before implementation starts.
- Contracts are defined before integration-heavy work begins.
- Every delivery slice must include a concrete validation path, preferably scriptable from the repo.
- Repo docs change in the same pull request as material behavior changes.
- Stale drafts are either promoted, replaced, or removed.

## Architecture Guardrails
- Prefer a modular monolith before introducing distributed services.
- Start event-driven from the first slice, but keep the event model minimal and business-focused.
- Keep clear bounded contexts even inside a single deployable.
- Separate public product flows, retailer operations, and asynchronous processing by code boundary.
- Treat privacy, tenant isolation, and auditability as first-order concerns.
- Use projections or dedicated read models for dashboard-style reads.
- Keep external systems behind narrow contracts.
- Treat white-label configuration as core product behavior, not front-end decoration.
- Treat wallet-card state as domain state, not a view-only convenience.

## Entropy Control
- Avoid duplicate documents that describe the same truth at different levels.
- Do not let plans absorb architecture notes or specs absorb product strategy.
- Remove copied artifacts from previous projects unless they are intentionally adapted.
- Prefer a small number of living documents over large archives of stale drafts.
- When a draft becomes authoritative, rename it and update inbound references.

## Review Cadence
- Review the active architecture note when a new bounded context, datastore, or deployment shape is introduced.
- Review the harness when the workflow itself starts causing friction or ambiguity.
- Review active plans and specs whenever scope or sequencing changes materially.

## Current Application
- Current business input: `docs/business/fic-business-plan-v1.docx`
- Current architecture draft: `docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md`
- Current active plan: `docs/plans/active/F00-inception.md`

## Near-Term Next Step
- extract the main architecture decisions into `RFC-001-platform-architecture.md`
- split the first buildable slice into a first spec rather than keeping it buried in the draft architecture document
- ensure the first spec includes validation scripts and evidence for the join, pass issuance, and visit-award path
