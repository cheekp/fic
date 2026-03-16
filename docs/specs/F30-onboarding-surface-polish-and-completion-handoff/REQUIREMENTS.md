# F30 Requirements

## Objective
- Onboarding surfaces must feel direct and production-oriented, with clear entry, payment, and completion-state communication.

## Functional Requirements
- Home must emphasize one primary acquisition action:
  - `Sign up now`
- Home must keep login reachable without competing with the primary signup action.
- Billing setup must expose payment-method intent in-flow and include:
  - Apple Pay visual treatment
  - card-entry fields for operators who expect manual card setup
- Billing must still preserve current self-serve plan guardrails (`starter` only in this slice).
- Workspace must show a clear completion handoff after first customer join unlocks non-first-time sections.
- Completion handoff must provide direct actions to newly unlocked areas (overview and insights).
- Merchant utility chrome must remove non-essential decorative support labels while preserving access to support destinations.

## UX Requirements
- Public entry lane should avoid stacked narrative blocks and secondary content that compete with signup intent.
- Payment setup copy must clearly communicate this is setup-time payment preference capture.
- Completion-state messaging must be obvious enough that a new operator understands onboarding is done.

## Operational Requirements
- Existing F29 onboarding validators and tests must remain green after this polish slice.
- New slice behavior must be covered by targeted component tests and a slice validator.
