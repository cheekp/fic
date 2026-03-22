# F36 Acceptance

- `src/Fic.Platform.Frontend/components/layout/portal-shell.tsx` exists and is used by signup/plan/billing/workspace routes.
- Mobile portal top bar includes burger interaction that opens a rail drawer.
- Desktop portal layout includes pinned rail navigation.
- `src/Fic.Platform.Frontend/components/layout/portal-surface.tsx` exists for reusable surface primitives.
- `src/Fic.Platform.Frontend/types/portal-contracts.ts` exists and defines nav/theme contracts plus FIC defaults.
- API-provided portal theme payload includes compiled brand tokens used directly by the Next.js shell and workspace/join brand surfaces.
- Existing onboarding/workspace route functionality still works.
- `npm run build` succeeds in `src/Fic.Platform.Frontend`.
- `scripts/validate-f36-portal-chrome-and-theme-contracts.sh` passes.
