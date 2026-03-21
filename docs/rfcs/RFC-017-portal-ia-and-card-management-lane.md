# RFC-017 Portal IA And Card Management Lane

## Problem

Portal hierarchy currently feels fragmented between public and operator surfaces, and customer card operations do not scale well beyond a small number of cards.

## Decision

Implement an F45 UX architecture slice that:

- standardizes public top chrome around a burger-first IA entry plus auth CTAs,
- removes competing navigation panes in merchant workspace composition,
- introduces a table-oriented customer card management lane with richer row metadata and practical actions.

## Scope

- Next.js public route header system (`/`, `/blogs`, `/training`, `/consultancy`, `/account`, `/billing`).
- Workspace composition cleanup in merchant route to avoid duplicate/competing navigation panes.
- Customers route redesign for at-scale card review and reward operations.
- No backend contract shape changes in this slice.

## Consequences

- Improves perceived product quality and coherence across entry and operator contexts.
- Creates a stable shell foundation for upcoming route growth without repeating IA divergence.
- Keeps API/domain contracts stable while materially upgrading UX and operability.
