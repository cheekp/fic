# F18 Real Wallet Demo

## Goal

Make the signed Apple Wallet founder demo easier to trust and easier to configure by surfacing readiness diagnostics and proving the `.pkpass` generation path with real tests.

## Why Now

- the merchant workspace is good enough to support a real founder demo
- the main remaining risk is whether the Wallet pass path works cleanly when signing material is provided
- current preview fallback messaging is too vague when Wallet signing is not fully configured

## Scope

- improve Wallet capability diagnostics in the application
- add a support/readiness surface for local Wallet demo setup
- add automated tests that build a signed `.pkpass` with generated certificates
- tighten the founder runbook and LAN demo script around readiness checks

## Planned Outcomes

- the app clearly explains why Wallet demo is ready or not ready
- founder setup is less guesswork-heavy
- the signed pass code path is covered by automated proof rather than only manual testing

## Exit Criteria

- `F18` spec exists with requirements and acceptance
- `F17` moves to completed plans
- the app exposes Wallet demo readiness diagnostics
- the Wallet pass service has automated `.pkpass` generation coverage
- validator and tests pass
