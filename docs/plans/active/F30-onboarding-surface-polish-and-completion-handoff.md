# F30 Onboarding Surface Polish And Completion Handoff

## Goal
- Reduce onboarding friction by making the public entry lane more direct, clarifying payment capture intent in setup, and making the post-onboarding unlock moment explicit.

## Scope
- Simplify home page messaging around one dominant CTA (`Sign up now`) with lower visual noise.
- Keep billing step focused while showing payment-method choice with visible Apple Pay treatment and card-entry fields in-flow.
- Merge first-time completion handoff into top workspace chrome once first template selection unlocks full sections.
- Use one shared segmented-control treatment for both workspace-level and programme-level tabs.
- Add section-level rail management for larger shops:
  - Active/Scheduled/Expired counts
  - per-section collapse/expand
  - rail sort control within sections
- Add reusable UX quality gates so polish changes stay consistent and reviewable:
  - component-level UX contract tests
  - stylesheet token and motion guard checks
  - optional Playwright browser-smoke overflow checks
- Add demo-seed actions to every onboarding step (account, plan, billing, shop details, programme template) behind the existing feature flag.
- Reduce utility-header clutter by removing non-essential lozenges and keeping mobile menu behavior consistent.
- Add a shared social-channel rail (Facebook, Instagram, TikTok, X) across public company surfaces, anchored from landing.

## Out Of Scope
- Real payment processor integration and subscription provisioning.
- Apple Pay merchant validation or production wallet payment handling.
- Broader post-onboarding workspace IA redesign beyond handoff clarity.

## Proof
- `scripts/validate-f30-onboarding-surface-polish.sh`
- `scripts/validate-ux-surface.sh`
