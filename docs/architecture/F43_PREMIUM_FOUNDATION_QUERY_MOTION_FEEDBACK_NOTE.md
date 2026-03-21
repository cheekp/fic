# F43 Premium Foundation Query/Motion/Feedback Note

## Intent

Establish a durable premium frontend foundation by standardizing server-state handling, live feedback, and motion hierarchy across signup and merchant workspace surfaces.

## Decisions

- Add TanStack Query as the canonical server-state layer for portal navigation and workspace reads.
- Add Sonner to provide consistent non-blocking success/error feedback in high-frequency onboarding and workspace actions.
- Keep Radix/shadcn as component primitives; add Vaul drawer usage where mobile utility navigation benefits from sheet interactions.
- Apply restrained Framer Motion transitions at shell/content level to improve perceived quality without introducing animation noise.

## Outcome

- Fewer ad hoc `useEffect` fetch loops and more predictable request lifecycle behavior.
- A reusable toast pattern for operator actions (save, upload, create, update, redeem).
- Improved premium polish from subtle transition hierarchy while preserving performance and clarity.
