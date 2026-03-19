# Next.js Design Pass Guardrails

## Purpose
Prevent UI/UX drift during iterative refinement of the Next.js frontend while preserving Blazor-aligned brand character and improving premium feel.

## Scope
- Applies to `src/Fic.Platform.Frontend/` surfaces.
- Priority routes for every pass:
  - `/`
  - `/portal/signup`
  - `/portal/signup/plan/[merchantId]`
  - `/portal/signup/billing/[merchantId]`
  - `/portal/merchant/[merchantId]`
- Focus areas: onboarding roadmap, signup flow, plan, billing, workspace chrome.

## Non-Negotiables
1. Brand alignment
- Use shared brand palette and tone from Blazor references.
- Preserve visual continuity with existing FIC identity (not generic template UI).

2. Functional parity protection
- Visual changes must not break signup progression, plan selection, billing completion, workspace entry.
- No regression to API interaction behavior.

3. Mobile-first density
- Mobile must prioritize task completion with minimal vertical waste.
- Any new visual flourish must justify its space cost.

4. Single source of progress truth
- Roadmap progress should not duplicate status in multiple heavy UI blocks.

## Roadmap Component Rules
1. Header
- Single-line header row only.
- Keep `Signup roadmap` and completion pill.
- Remove redundant explanatory lines (for example `Current step: ...`).

2. Vertical footprint
- Keep compact spacing and avoid stacked helper text.
- Avoid introducing large empty regions above/below node rail.

3. Node semantics
- Round indicator uses number when pending/current, check when complete.
- Labels remain legible but compact on mobile.
- Mobile may hide non-current labels if needed for density.

4. Interaction
- Completed/current steps may be links.
- Non-complete future steps remain non-interactive.
- Interactive nodes require visible keyboard focus ring.

5. Accessibility
- Current step link must include `aria-current="step"`.
- Color/contrast should remain clear at a glance.

## Visual Refinement Budget
Each pass should change at most one major and two minor visual axes.

Major axes:
- Layout/hierarchy
- Typography scale
- Component density
- Surface styling
- Motion behavior

Minor axes:
- Spacing/token tuning
- Border/shadow tuning
- Icon/chip treatments
- Label copy tightening

If a pass exceeds this budget, split into separate slices.

## Definition of Done for a Design Pass
1. Functional checks
- Signup create account submits and routes to plan.
- Plan route progresses to billing.
- Billing route progresses to workspace.
- No blocking runtime errors in target flow.

2. Visual checks
- Capture current screenshots for desktop + mobile target routes.
- Compare against baseline and review diffs manually.
- Confirm roadmap density and readability at mobile width.

3. Technical checks
- `npm run build` passes in `src/Fic.Platform.Frontend`.
- No unresolved console/runtime errors introduced by the pass.

## Required QA Commands
Run from `src/Fic.Platform.Frontend`.

```bash
npm run qa:signup-flow
npm run qa:visual-baseline:capture
npm run qa:visual-baseline:compare
npm run build
```

If baseline changes are intentional, document rationale in the change summary before accepting the new baseline.

## Baseline Governance
- Baselines live under `src/Fic.Platform.Frontend/visual-baseline/`.
- Do not refresh baseline images for unrelated UI changes.
- Any baseline update must include:
  - what changed
  - why it is better
  - which guardrail(s) it improves

## Change Log Template (Use in PR/summary)
```text
Design Pass: <short title>
Routes touched: <list>
Major axis changed: <one>
Minor axes changed: <up to two>
Guardrails validated:
- [ ] Brand alignment
- [ ] Functional parity
- [ ] Mobile density
- [ ] Roadmap rules
QA evidence:
- qa:signup-flow: pass/fail
- visual compare summary: <path>
- build: pass/fail
Notes:
- <intentional baseline updates>
```

## Escalation Rule
Pause and request alignment before implementation if a proposed change would:
- increase roadmap vertical footprint significantly,
- remove established brand character,
- trade functional clarity for visual novelty,
- or require repeated baseline resets without measurable UX improvement.
