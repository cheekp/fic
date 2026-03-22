---
name: frontend-visual-qa
description: Run post-change visual QA for the FIC Next.js frontend. Use when React, CSS, branding, layout, or portal chrome changes may have affected visual quality, responsiveness, or brand alignment.
---

# Frontend Visual QA

Use this skill after changes to:
- `src/Fic.Platform.Frontend/app/**`
- `src/Fic.Platform.Frontend/components/**`
- `src/Fic.Platform.Frontend/lib/brand.ts`
- theme contracts, hero copy, layout, navigation, or visual styling

## Workflow

1. Read the current brand guide:
   - `docs/business/brand-guidlines.txt`
2. Read the checklist:
   - `references/brand-qa-checklist.md`
3. Run the wrapper:

```sh
.agents/skills/frontend-visual-qa/scripts/run-visual-qa.sh
```

Optional modes:

```sh
.agents/skills/frontend-visual-qa/scripts/run-visual-qa.sh build
.agents/skills/frontend-visual-qa/scripts/run-visual-qa.sh signup
.agents/skills/frontend-visual-qa/scripts/run-visual-qa.sh workspace
.agents/skills/frontend-visual-qa/scripts/run-visual-qa.sh baseline
```

## What To Check

- Hero contrast and readability in mobile and desktop screenshots
- CTA prominence and button legibility
- Horizontal overflow or clipped content
- Visual hierarchy: headline, supporting copy, CTA, and imagery should read clearly
- Brand fit against North Star Customer Solutions guidance
- Regressions in signup and workspace flows

## Reporting

Report findings first. Include:
- severity
- route
- viewport
- artifact path
- why it violates the checklist or brand guidance

If no findings are discovered, say that explicitly and mention which artifacts were reviewed.
