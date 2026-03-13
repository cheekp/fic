# F00 Inception

Completed once the repo moved from inception docs into implemented delivery slices (`F01` onward).

## Goal
Turn the business plan and the initial technical architecture into the first buildable delivery slice.

## Inputs
- `docs/ENGINEERING_HARNESS.md`
- `docs/business/fic-business-plan-v1.docx`
- `docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md`

## Current Technical Constraints
- cloud-native architecture
- event-driven architecture from the first meaningful slice
- Docker-based local development and test workflow
- Azure target for stage and production
- PWA product delivery
- Apple Wallet first
- Cosmos DB currently preferred, but must be proven workable for local Docker-based development
- white-label retailer configuration for both the PWA and wallet pass
- business plan remains the source of truth for product direction

## Expected Outputs
- one reviewed architecture note in `docs/architecture/`
- one inception RFC in `docs/rfcs/`
- one first implementation spec in `docs/specs/`
- one clear validation path for the first slice

## Current Proposed First Slice
- internal MVP/demo for vendor sign-up, loyalty-programme configuration, QR join, Apple Wallet issuance, and visible progress state

## Open Questions
- What is the narrowest workflow worth building first?
- Is Cosmos DB local emulation good enough for day-to-day development, or do we need a shared Azure dev environment?
- What is the minimum retailer configuration model needed to drive both branding and wallet-card behavior?
- Which risks must be validated before writing production code?

## Exit Criteria
- the initial architecture is documented
- the engineering harness is agreed as the repo operating model
- the inception RFC is agreed
- the first implementation spec has requirements and acceptance criteria
- the first slice has a scriptable validation approach
