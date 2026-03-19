# F35 Acceptance

- `src/Fic.Platform.Frontend/tailwind.config.ts` and `postcss.config.mjs` exist and are active.
- `src/Fic.Platform.Frontend/components/ui/` includes button/card/input/label/tabs/select/badge primitives.
- Signup, plan, billing, and workspace Next routes use shared UI primitives.
- `src/Fic.Platform.Frontend/app/manifest.ts`, `public/sw.js`, and `app/offline/page.tsx` exist.
- Next.js UI refinement passes follow `docs/runbooks/NEXTJS_DESIGN_PASS_GUARDRAILS.md`.
- Design-pass QA evidence includes `npm run qa:signup-flow`, `npm run qa:visual-baseline:capture`, and `npm run qa:visual-baseline:compare` before baseline updates are accepted.
- `npm run build` succeeds in `src/Fic.Platform.Frontend`.
- `dotnet test` for `MerchantApiTests` and `MerchantAuthBoundaryTests` still passes.
- `scripts/validate-f35-nextjs-premium-pwa-ui-system.sh` passes.
