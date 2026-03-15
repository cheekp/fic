# F24 Requirements

## Objective
- The Wallet founder demo must feel complete from the merchant and customer points of view, including a clear post-stamp pass refresh story.

## Functional Requirements
- The repo must expose Wallet pass issuance readiness separately from Wallet refresh readiness.
- The app must support a Wallet refresh notifier seam that can request pass updates for registered devices after:
  - visit award
  - reward redeem
- Wallet refresh requests must be driven by registered device push tokens already captured through the Wallet web-service registration flow.
- Merchant-facing workflow messaging must clearly report whether a Wallet refresh request was:
  - sent
  - skipped because no registered devices exist yet
  - unavailable because push delivery is not configured
- Company support surfaces must explain both:
  - signed pass issuance readiness
  - Wallet refresh readiness
- The local founder demo runbook must describe the full critical path from signup through pass update.

## Founder Demo Constraints
- Apple Wallet remains the only live customer delivery output.
- APNs delivery may use the existing pass certificate path for local founder demo readiness.
- If push delivery is disabled or incomplete, the UI must say so clearly instead of implying automatic refresh is working.
