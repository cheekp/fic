# Documentation Map

This repo keeps raw business inputs separate from technical decisions and implementation slices.

## Structure
- `business/` source business material from the founders
- `brand-examples/` visual reference material for sample vendors and white-label design exploration
- `ENGINEERING_HARNESS.md` repo-level rules for architecture discipline and entropy management
- `architecture/` system context, technical architecture, and integration notes
- `runbooks/` operational guides for local setup, demo flows, and support tasks
- `rfcs/` proposals and decisions that shape product and system direction
- `plans/active/` the current delivery plan
- `plans/planned/` queued work that is not active
- `plans/completed/` closed work records
- `specs/` buildable slices with requirements and acceptance
- `../scripts/` validation scripts and workflow helpers

## Naming
- RFCs: `RFC-###-slug.md`
- Plans: `F##-slug.md`
- Specs: `F##-slug/REQUIREMENTS.md` and `F##-slug/ACCEPTANCE.md`

## Rule Of Thumb
Business documents describe why the company should exist.
The engineering harness describes how the repo stays coherent as it grows.
Architecture documents describe how the system should work.
RFCs describe what decision is being proposed.
Specs describe what must be built and how it will be accepted.
