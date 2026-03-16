# F30 Acceptance

- Home presents a dominant `Sign up now` CTA with reduced clutter.
- Home still provides login access without diluting signup focus.
- Home and company-support/account pages surface Facebook, Instagram, TikTok, and X links with shared icon styling.
- Billing shows Apple Pay visual treatment and in-flow card-entry fields.
- Billing still routes only Starter self-serve forward in onboarding.
- First-time workspace transition shows explicit `Setup complete` handoff messaging inline in top chrome once the first template is chosen and full workspace is unlocked.
- Completion handoff provides direct links to `Overview` and `Insights`.
- Top workspace tabs and programme tabs share one segmented-control visual system.
- Programme rail shows grouped counts for `Active`, `Scheduled`, and `Expired`, with per-group collapse/expand controls.
- Programme rail provides sort selection for ordering programmes inside each lifecycle group.
- Demo seed actions are present on all onboarding steps when `Features:SignupDemoSeedEnabled` is enabled.
- Merchant utility bar no longer renders the `Support by North Star` lozenge.
- Mobile utility menu remains usable and layered above page content.
- Targeted tests and F30 validator pass.
- `scripts/validate-ux-surface.sh` passes with UX contract checks.
- When `FIC_UX_BROWSER_SMOKE=1` is set, Playwright smoke checks run and produce screenshots under `artifacts/ux-smoke/`.
