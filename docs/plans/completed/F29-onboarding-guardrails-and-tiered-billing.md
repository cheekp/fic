# F29 Onboarding Guardrails And Tiered Billing

## Goal
- Make first-run onboarding feel production-ready by enforcing a clean guided path (`signup -> plan -> billing/access -> shop details -> programme template`) with less clutter, while preserving a feature-flagged demo shortcut for seeded data.

## Scope
- Remove default fake signup values and introduce a feature-flagged `Use demo details` action.
- Split early onboarding so signup captures lead details first (owner email + shop name), followed by a dedicated plan step, then billing/access, with shop details completed afterward.
- Keep a persistent onboarding journey indicator visible across signup, billing, and first-time programme setup.
- Tighten onboarding guardrails so billing confirmation and supported plan checks are enforced before workspace entry.
- Replace fake card-entry fields with faux product tiers that communicate roadmap depth (`SSO`, `CRM integrations`, `consultancy`) while keeping only one enabled self-serve tier.
- Keep `£19.99/mo` as the only enabled self-serve path in this slice.

## Out Of Scope
- Real payment processing and subscription provisioning.
- Real SSO or CRM integrations.
- Post-first-join workspace redesign.

## Proof
- `scripts/validate-f29-onboarding-guardrails.sh`
