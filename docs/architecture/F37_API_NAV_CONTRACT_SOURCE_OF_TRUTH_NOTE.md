# F37 API Nav Contract Source Of Truth Note

## Intent

Move portal route-slice badges/completion and shell nav state to an API contract so the frontend consumes one server-owned navigation model.

## Decision

- Add versioned API contract endpoint(s) for signup/workspace shell navigation.
- Build nav state from domain-backed workspace snapshot and setup checklist.
- Return theme contract from API with merchant brand override + safe fallback.
- Keep frontend rendering-only for nav contract consumption.

## Endpoints

- `GET /api/v1/portal/navigation/signup?step=<signup|plan|billing>&merchantId=<optional-guid>`
- `GET /api/v1/merchants/{merchantId}/portal/navigation?step=<operate|configure|customers>&programmeId=<optional-guid>`

## Consequences

- Nav state rules now evolve in one place in C#.
- Next.js route slices stop deriving completion/disabled rules locally.
- Theme fallback behavior is consistent across signup/workspace shells.
