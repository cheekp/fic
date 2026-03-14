# FIC

Founding repository for the FIC startup.

## Current Inputs
- Business plan: `docs/business/fic-business-plan-v1.docx`
- Technical architecture draft: `docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md`
- Engineering harness: `docs/ENGINEERING_HARNESS.md`

## Working Method
1. Capture business and technical intent in docs first.
2. Use RFCs to align on product and architecture decisions.
3. Break approved decisions into buildable specs with explicit requirements and acceptance.
4. Implement in small slices and keep the docs current with the code.

## Repository Layout
- `docs/business/` source business material
- `docs/ENGINEERING_HARNESS.md` repo-level operating harness for entropy control and delivery discipline
- `docs/architecture/` system architecture and technical notes
- `docs/runbooks/` operator-focused setup guides for local demos and support workflows
- `docs/rfcs/` proposals under review
- `docs/plans/` active, planned, and completed delivery records
- `docs/specs/` implementation-ready requirements and acceptance
- `scripts/` slice-level validation and workflow helpers
- `tests/` automated verification as the codebase emerges

## Next Step
Finish the real signed Apple Wallet founder demo from here: improve readiness diagnostics, prove `.pkpass` generation with automated tests, then wire pass updates and stronger merchant auth/session handling.
