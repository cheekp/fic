# F28 Acceptance

- First-time merchants cannot remain on deep links to `section=insights` or `programmeSection=customers`; the route is normalized back to first-time allowed sections.
- First-time top-level navigation shows only `Programmes`.
- First-time programme sub-navigation shows only `Operate` and `Configure`.
- First-time `Operate` shows join and stamp actions plus one next-step prompt, without secondary timeline or summary panels.
- After at least one customer join exists, advanced navigation sections return.
- Shop settings drawer remains available in first-time mode.
- Workspace component tests cover first-time route gating redirects and post-join unlock behavior.
