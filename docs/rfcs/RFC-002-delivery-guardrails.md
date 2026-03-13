# RFC-002 Delivery Guardrails

## Status
Proposed

## Problem

The current loop delivers quickly but we repeatedly discover similar quality issues late in a slice:

- UI interaction regressions are protected after polish, not before it.
- Cross-surface changes (workspace/join/wallet/brand) can spread across projects without explicit seam ownership.
- Local reliability issues are found during coding instead of with an upfront preflight.

## Context

Recent implemented slices proved:

- strong spec discipline and validator usage
- growing UI complexity in merchant workspace flows
- increasing need for repeatable quality checks beyond compile/test only

## Options Considered

### Option A: Keep current approach and rely on ad-hoc discipline
- Pros: no additional process overhead
- Cons: repeated churn and late discovery continue

### Option B: Introduce lightweight scripted guardrails (chosen)
- Pros: low overhead, automatable, compatible with current harness
- Cons: requires team habit updates and validator maintenance

### Option C: Introduce heavyweight CI policy gates immediately
- Pros: stronger central enforcement
- Cons: too heavy for current stage and likely to slow iteration

## Decision

Adopt a lightweight delivery-guardrails slice with:

- interaction-contract-first testing for UI-heavy slices
- seam checklist capture in slice specs
- local preflight reliability script
- periodic churn hotspot reporting

## Consequences

### Positive

- earlier detection of UI regressions
- clearer ownership of cross-surface boundaries
- fewer avoidable local setup interruptions
- improved signal on where refactors should happen

### Negative

- adds a small upfront step before implementation work
- scripts require upkeep as project structure evolves

## Open Questions

- should preflight become mandatory in all slice validators or remain optional guidance
- what churn threshold should trigger mandatory refactor work
- when should these checks move into CI rather than local-only workflow
