# F30 Requirements

## Objective
- Onboarding surfaces must feel direct and production-oriented, with clear entry, payment, and completion-state communication.

## Functional Requirements
- Home must emphasize one primary acquisition action:
  - `Sign up now`
- Home must keep login reachable without competing with the primary signup action.
- Home and company-support surfaces must expose social-channel links (Facebook, Instagram, TikTok, X) using consistent icon treatment.
- Billing setup must expose payment-method intent in-flow and include:
  - Apple Pay visual treatment
  - card-entry fields for operators who expect manual card setup
- Billing must still preserve current self-serve plan guardrails (`starter` only in this slice).
- Workspace must show a clear completion handoff in top chrome once first programme template selection unlocks non-first-time sections.
- Completion handoff must provide direct actions to newly unlocked areas (overview and insights).
- Workspace top-level tabs and programme-level tabs must use one shared segmented-control style system.
- Programme rail must support section management for multi-programme shops:
  - grouped counts for Active/Scheduled/Expired
  - collapse/expand at section level
  - sort control for programme ordering inside groups
- Demo seed actions must be available at all onboarding steps when `Features:SignupDemoSeedEnabled` is on.
- Merchant utility chrome must remove non-essential decorative support labels while preserving access to support destinations.

## UX Requirements
- Public entry lane should avoid stacked narrative blocks and secondary content that compete with signup intent.
- Social links should read as a compact utility rail, not a competing secondary CTA block.
- Payment setup copy must clearly communicate this is setup-time payment preference capture.
- Completion-state messaging must be obvious enough that a new operator understands onboarding is done.

## Operational Requirements
- Existing F29 onboarding validators and tests must remain green after this polish slice.
- New slice behavior must be covered by targeted component tests and a slice validator.
- UX quality gates must be scriptable and repeatable:
  - contract tests for navigation hierarchy and CTA prominence
  - style guards for shared control tokens and reduced-motion fallback
  - optional browser-smoke overflow checks at core breakpoints
- UX quality gates must be runnable through `scripts/validate-ux-surface.sh`.
