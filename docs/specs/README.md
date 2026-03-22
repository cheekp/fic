# Specs

Use specs for buildable slices that are ready to move into implementation.

Status does not live here.
Use `docs/plans/active/`, `docs/plans/planned/`, and `docs/plans/completed/` for workflow state.
Keep `docs/specs/` flat by slice so old contracts stay easy to find without carrying duplicate status folders.

## Suggested Structure
- `F##-slug/REQUIREMENTS.md`
- `F##-slug/ACCEPTANCE.md`
- `F##-slug/DESIGN.md` when a slice needs local design reasoning that does not belong in durable architecture docs
- `F##-slug/contracts/` when a slice has payload, API, or interface contracts

## Rule
If a slice is too vague to state acceptance criteria, it is still architecture or RFC work and should not be opened as a spec yet.

If a note only explains how one slice should be shaped or composed, keep it with that slice under `docs/specs/` rather than adding another file under `docs/architecture/`.
