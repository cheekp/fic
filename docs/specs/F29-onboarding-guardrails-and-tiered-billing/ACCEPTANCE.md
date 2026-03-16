# F29 Acceptance

- Signup renders empty merchant fields by default.
- Signup asks only for shop name and owner email.
- When the demo-seed feature flag is enabled, signup shows a `Use demo details` action that fills merchant form defaults.
- Plan selection is a dedicated onboarding step between signup and billing.
- Billing no longer requests fake card-number entry.
- Billing captures payment preference and owner access after plan selection, while still rejecting unsupported posted plans.
- Plan selection displays faux Growth/Enterprise options with roadmap capabilities and contact-sales positioning.
- Only the `£19.99/mo` self-serve tier advances into workspace setup.
- First-time workspace keeps shop-details completion ahead of first programme creation.
- Branding controls are available in shop settings during onboarding but can be skipped.
- A persistent onboarding journey indicator appears on signup, billing, and first-time workspace setup.
- Billing tiers and onboarding journey surfaces present clear visual distinction between required self-serve actions and contact-sales options.
- Targeted component/state tests and the F29 validator pass.
