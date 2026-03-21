# F43 Requirements

## Functional

1. Frontend root layout provides a shared TanStack Query client.
2. Portal signup/workspace navigation reads use TanStack Query hooks instead of local effect-only fetch loops.
3. Workspace bootstrap read path uses TanStack Query with existing route behavior preserved.
4. Sonner toaster is available app-wide and used for key onboarding/workspace mutation outcomes.
5. Portal shell supports mobile utility links via drawer interaction using Vaul.
6. Shell/content transitions use restrained Framer Motion animation with no blocking impact.

## Non-Functional

1. Existing API contracts and route URLs remain unchanged.
2. Motion remains subtle and does not create layout shift or interaction delays.
3. Build and existing QA scripts remain green.
