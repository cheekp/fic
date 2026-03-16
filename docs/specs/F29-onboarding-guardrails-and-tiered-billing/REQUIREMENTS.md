# F29 Requirements

## Objective
- First-time onboarding must be simple, guarded, and credible for production conversations without losing demo speed.

## Functional Requirements
- Signup must be a lead-capture step that collects only:
  - shop name
  - owner email
- Signup must not pre-populate fake merchant values by default.
- Signup may expose a seeded-data shortcut only when a feature flag is enabled.
- The onboarding journey indicator must persist across:
  - signup
  - plan selection
  - billing
  - shop details
  - first-time programme setup in the merchant workspace
- The onboarding flow must keep commercial choices in sequence:
  - choose plan tier
  - choose payment method and owner access
  - complete shop details
  - choose first programme template
- Shop details must be completed after billing and before the first programme can be created.
- Branding controls must be available during shop setup but remain optional/skippable.
- Billing completion must enforce:
  - supported self-serve plan selection
- If owner access credentials were already configured, billing must not silently reconfigure them.
- Plan selection must present multiple product tiers, but only one tier is enabled for self-serve in this slice.

## UX Requirements
- Onboarding copy must avoid fake payment data capture that is not processed.
- Tier presentation must communicate product depth (for example `SSO`, `CRM integrations`, `consultancy`) while clearly labeling contact-sales tiers.
- Onboarding surfaces must maintain clear visual hierarchy with persistent progress context, so required actions are obvious at a glance.
- First-time workspace entry must keep programme-template setup as the primary action.

## Operational Requirements
- The demo-seed shortcut must remain off by default in baseline app settings.
- The shortcut must be enableable through configuration for demos.
