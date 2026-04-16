# PR Review Evidence Guide

Use this guide when opening PRs for high-risk frontend or contract work.

It turns recent review pain into a repeatable checklist so we catch regressions before merge rather than through same-day fix passes.

## Why This Exists

Recent merged work showed three repeated failure modes:

- visual regressions on homepage, signup, and portal chrome after larger brand/layout passes
- contract drift risk when backend-owned payloads changed across C#, API builders, frontend types, and tests
- wide PR scope on already-hot files, which made review slower and follow-up fixes more likely

The related repo decisions are already captured in:

- `docs/rfcs/RFC-002-ux-quality-gates.md`
- `docs/rfcs/RFC-003-css-budget-and-tokenization-discipline.md`
- `docs/rfcs/RFC-009-api-nav-contract-source-of-truth.md`

## When To Use It

Use this checklist for PRs that touch any of the following:

- `src/Fic.Platform.Frontend/app/page.tsx`
- `src/Fic.Platform.Frontend/app/portal/signup/**`
- `src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx`
- `src/Fic.Platform.Frontend/components/layout/portal-shell.tsx`
- `src/Fic.Platform.Frontend/components/layout/public-portal-header.tsx`
- shared contract files in `src/Fic.Contracts/`, `src/Fic.Platform.Web/Services/`, `src/Fic.Platform.Frontend/types/`, or API tests

## 1. Visual Triage For Hotspot Routes

When a PR changes homepage, signup, portal chrome, or other major visual surfaces:

1. Run the frontend visual QA wrapper:

```sh
.agents/skills/frontend-visual-qa/scripts/run-visual-qa.sh
```

2. If the pass is design-heavy, also run the stronger route-specific QA noted in `docs/runbooks/NEXTJS_DESIGN_PASS_GUARDRAILS.md`.
3. Add desktop and mobile evidence for the affected route set in the PR description.
4. If a baseline or screenshot changed intentionally, say what changed and why it is better.

Minimum evidence for a design-heavy PR:

- affected routes
- desktop and mobile artifact paths
- whether browser smoke or baseline compare ran
- any intentional baseline refresh rationale

## 2. Contract Triple-Lock

When a PR changes an API-owned contract, review should confirm all four legs moved together:

1. shared contract shape
2. backend mapping or endpoint payload builder
3. frontend consumer or type definition
4. API or integration proof

Typical file groups:

- `src/Fic.Contracts/*.cs`
- `src/Fic.Platform.Web/Services/*.cs`
- `src/Fic.Platform.Frontend/types/*.ts`
- `tests/Fic.Platform.Web.Tests/*ApiTests.cs`

If one leg is intentionally deferred, the PR should explain the compatibility seam explicitly.

## 3. Scope Budget For High-Volatility Files

Keep PRs narrow when they touch repeat-hotspot files.

Preferred budget:

- one major high-volatility surface change
- supporting files needed to complete that slice
- no unrelated copy polish or secondary layout rewrites in the same PR

If a PR goes wider than that, the description should explain:

- why the scope could not be split safely
- what the primary review focus is
- which parts are most regression-prone

## PR Template

Use `.github/pull_request_template.md` so the review evidence stays consistent across slices.
