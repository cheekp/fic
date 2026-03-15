# F27 Acceptance

- Wallet push dispatch reports explicit outcome categories for success, retryable failure, and permanent failure.
- Invalid or expired push tokens are not retried indefinitely after permanent APNs failure responses.
- Merchant-facing stamp and redeem feedback indicates whether refresh was sent, skipped, retry-needed, or unavailable.
- Support Wallet surface includes concise push troubleshooting context aligned to dispatch outcomes.
- Wallet demo runbook includes a short troubleshooting checklist for missed refresh behavior.
- Existing onboarding, auth, programme, and Wallet issuance or update tests continue to pass.
- New automated tests cover APNs outcome classification and token lifecycle cleanup rules.
