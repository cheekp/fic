# F35 Requirements

## Objective

Ship a premium mobile-first PWA shell for Next.js route slices without breaking existing C# or Blazor workflows.

## Functional Requirements

- Next frontend must use Tailwind CSS with tokenized visual variables.
- Shared UI primitives must exist for button/card/input/label/tabs/select/badge patterns.
- Route slices (`signup`, `plan`, `billing`, `workspace`) must consume shared primitives.
- Workspace must use Radix-powered tabs/select for section/programme interactions.
- PWA artifacts must include manifest, service worker registration, and offline fallback route.

## Operational Requirements

- `npm run build` for `src/Fic.Platform.Frontend` must pass.
- Existing API/auth test flows must continue passing.
- Blazor app startup/build path must remain unaffected by frontend additions.
