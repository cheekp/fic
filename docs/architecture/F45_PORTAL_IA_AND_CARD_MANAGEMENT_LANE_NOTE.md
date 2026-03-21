# F45 Portal IA + Card Management Lane Note

## Intent

Strengthen the Next.js operator experience by clarifying top-level portal hierarchy and introducing a scalable customer card management surface for daily use.

## Decisions

- Introduce a reusable public portal header with a left-side burger entry for site hierarchy and right-side auth actions.
- Keep merchant workspace chrome focused: no competing side-rail pane in the primary workflow composition.
- Move customer-card handling from stacked card snippets to a table-first management lane with card preview, status/progress metadata, and row actions.
- Keep roadmap/setup progression contract-driven while reducing visual competition with workspace operation controls.

## Outcome

- Clearer information architecture across public and operator surfaces.
- Faster card operations at higher volumes with less scroll and less cognitive switching.
- Better long-term shell foundation for additional top-level routes (blogs, training, consultancy, account, billing) without UX drift.
