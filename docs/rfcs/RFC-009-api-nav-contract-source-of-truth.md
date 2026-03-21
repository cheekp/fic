# RFC-009 API Nav Contract Source Of Truth

## Problem

Portal navigation badges/completion and gating rules are currently computed in frontend adapters, creating drift risk between route slices and backend state.

## Decision

Introduce API-owned portal nav contracts and migrate Next.js to consume them directly.

## Scope

- New contract records in `Fic.Contracts` for portal nav + theme.
- New API endpoints for signup and merchant workspace nav contracts.
- New backend builder service that maps workspace/checklist state to nav contract.
- Frontend route slices call API contract endpoints and stop local nav derivation.
- API integration tests for auth + payload shape.

## Consequences

- Stronger DDD boundary: domain/application state remains source of truth.
- Lower frontend logic duplication and less entropy over future slices.
- Slightly more API calls per page load, accepted for consistency in this slice.
