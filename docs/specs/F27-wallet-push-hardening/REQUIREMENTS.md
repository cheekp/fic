# F27 Requirements

## Objective
- Wallet refresh requests must be more resilient and transparent so a merchant can trust post-stamp pass updates and support can diagnose failures quickly.

## Functional Requirements
- APNs dispatch responses must be classified into explicit outcomes:
  - success
  - retryable failure
  - permanent failure
- Permanent APNs token failures must be removed from the active push-token set for future refresh attempts.
- Retryable APNs failures must surface as retry-needed outcomes without implying that the pass was updated.
- Merchant workspace feedback after stamp or redeem must report refresh result using one concise status line.
- Support Wallet surfaces must expose enough push-delivery detail to troubleshoot without reading server logs.
- Existing pass issuance and pass-update endpoints must remain backward-compatible.

## Operational Requirements
- Push capability checks must distinguish certificate/signing readiness from push transport readiness.
- Runbook guidance must include a short "if refresh did not arrive" checklist tied to surfaced statuses.
- New push-hardening logic must be covered by automated tests for classification and token-hygiene behavior.
