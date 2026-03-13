# FIC Delivery Guardrails Note

## Purpose

Capture a lightweight architecture for delivery guardrails that reduce rework in UI-heavy slices and tighten local reliability.

## Problem

Recent slices landed successfully but showed repeated churn in the same UI surfaces, late component-level test additions, and local environment fixes discovered during implementation instead of before it.

## Guardrail Architecture

### 1. Interaction Contract Layer

- Define a small interaction contract for each UI-heavy slice before polish work.
- Implement those contracts with focused bUnit tests plus any required state tests.
- Keep contracts behaviour-oriented (visibility, gating, selected-context rules), not snapshot-oriented.

### 2. Seam Checklist Layer

- Before coding, each slice records seam ownership for cross-surface behaviour.
- Each seam entry identifies:
  - owning context
  - surface consumers
  - interface boundary
  - tests at the boundary
- Multi-project edits are expected to map back to an explicit seam entry.

### 3. Local Reliability Layer

- Run a preflight script before coding changes.
- Preflight validates baseline launch profiles, static web assets, and SSR form naming discipline.
- Fail fast when local config drift is detected.

### 4. Churn Feedback Layer

- Generate a hotspot report from recent git history.
- Use the report to trigger targeted refactors when a file repeatedly accumulates change.

## Evidence Inputs

- Merchant workspace flow refinement required multiple iterative commits before stabilising.
- bUnit coverage arrived in the follow-up slice rather than being front-loaded.
- Early local setup work included several configuration and launch fixes.
- A small set of UI files absorbed repeated edits across many PRs.

## Operational Implications

- Delivery remains lightweight and script-first.
- Guardrails are intentionally simple shell scripts and markdown templates.
- The guardrails improve consistency without introducing heavyweight tooling.
