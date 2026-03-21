# F37 Requirements

## Functional

1. System provides a versioned HTTP API contract for portal signup navigation.
2. System provides a merchant-scoped HTTP API contract for portal workspace navigation.
3. Workspace nav endpoint enforces existing merchant ownership/auth boundary.
4. Nav contract includes item key, label, href, completion, disabled state, and optional badge.
5. Nav contract includes portal theme contract with merchant brand override and safe fallback defaults.
6. Next.js portal slices consume nav contract from API rather than deriving nav states locally.

## Non-Functional

1. Contract responses remain stable and typed via shared `Fic.Contracts` records.
2. Existing signup/workspace user flows remain functional.
