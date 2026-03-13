# F09 Delivery Guardrails Requirements

## Objective

Codify a lightweight guardrail set that catches common delivery issues earlier without slowing small-slice implementation.

## Functional Requirements

### Local Preflight

- The repo must provide a scriptable local preflight command.
- The preflight must fail clearly when required launch settings files are missing.
- The preflight must verify expected static web asset roots exist.
- The preflight must detect Razor files that use `EditForm` without a `FormName` declaration.

### Churn Hotspot Reporting

- The repo must provide a script to report top changed files from recent commit history.
- The script must support a configurable commit window.
- The output must be concise and suitable for plan/refactor discussions.

### Seam Checklist Template

- The repo must provide a seam checklist template that can be copied into new specs.
- The template must require:
  - owning context
  - interface boundary
  - surface consumers
  - boundary test location

### Slice Validator

- The repo must provide a validator script for this slice.
- The validator must assert the new scripts and template exist and are executable/readable.
- The validator must run preflight and churn report commands successfully.

## Non-Goals

- introducing full browser end-to-end tooling
- introducing mandatory CI policy gates for every guardrail in this slice
- replacing existing slice validators
