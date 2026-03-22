# AGENTS.md

Repository-specific instructions for agents working in this repo.

## Scope
- Applies only to this repository.

## Project Frame
- FIC is an inception-stage startup project.
- Current authoritative inputs are:
  - `docs/ENGINEERING_HARNESS.md`
  - `docs/business/fic-business-plan-v1.docx`
  - `docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md`

## Working Method
- Use lightweight spec-driven delivery.
- Sequence work in this order:
  1. Capture durable architecture in `docs/architecture/`
  2. Write or update the relevant RFC in `docs/rfcs/`
  3. Track the current delivery slice or in-flight set in `docs/plans/active/`
     - use `docs/plans/SLICE_INTAKE_TEMPLATE.md` when opening or re-scoping a slice
  4. Create implementation specs in `docs/specs/`
  5. Implement and prove behavior with tests

## Minimal Context Load Order
- `AGENTS.md`
- `README.md`
- `docs/ENGINEERING_HARNESS.md`
- the active architecture note in `docs/architecture/`
- the active RFC in `docs/rfcs/`
- the relevant in-flight plan in `docs/plans/active/`
- the relevant spec in `docs/specs/`

## Conventions
- Keep business inputs separate from derived technical artifacts.
- Treat the engineering harness as the repo-level system of record for workflow, layering, and entropy control.
- Prefer short, reviewable slices over large speculative plans.
- Each spec should define explicit requirements and acceptance criteria.
- Use `docs/architecture/` for durable system shape and `docs/specs/<slice>/DESIGN.md` for slice-local design notes.
- After meaningful frontend visual changes, use the repo-local skill `.agents/skills/frontend-visual-qa/` to run screenshot-based QA and brand checks against `docs/business/brand-guidlines.txt`.
- If behavior or architecture changes, update the corresponding RFC or spec in the same change.
- Use branch names like `rfc/<slug>`, `feature/<slug>`, `bug/<slug>`, and `chore/<slug>`.
