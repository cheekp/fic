# F18 Real Wallet Demo Requirements

## Objective

Turn the existing Apple Wallet signing seam into a founder-friendly demo path by surfacing exact readiness diagnostics and adding automated proof for signed .pkpass generation.

## Functional Requirements

### Readiness Diagnostics

- the Wallet capability model should expose specific readiness issues when signing is unavailable
- the merchant workspace should surface when the environment is still in preview fallback and link to Wallet demo setup help
- the application should expose a company-support Wallet demo page that explains the current capability and any missing inputs

### Founder Demo Path

- the LAN demo script should point the founder to a Wallet readiness check as well as merchant signup
- the local Wallet runbook should reference the in-app readiness surface

### Automated Proof

- the Wallet pass service should have automated tests that exercise signed `.pkpass` generation with generated certificates
- the automated proof should validate that the pass archive contains the expected package structure
- the automated proof should also cover readiness diagnostics for misconfigured signing material

## Non-Goals

- Apple push registration or pass refresh after visit award
- production secret storage or certificate rotation
- redesigning the merchant workspace hierarchy again
