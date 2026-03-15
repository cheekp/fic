# F26 Requirements

## Objective
- The first-programme experience must behave like a dedicated launch flow, not a normal programme workspace with extra setup blocks attached to it.

## Functional Requirements
- A merchant with zero programmes must see one full-width first-programme launch surface inside `Programmes`.
- The zero-programme launch surface must not render the normal programme rail alongside duplicate empty-state copy.
- Billing must continue the merchant into first-programme creation, but normal workspace navigation must not preserve a sticky `launch` state.
- Choosing the first programme template must open normal programme configuration for the new programme.
- Saving the first programme configuration must hand the merchant into normal `Operate` without preserving launch state.
- The first-programme setup strip must appear once at the top of `Programmes` while the merchant is still finishing the first programme.
- The setup strip must disappear once the first programme is ready for normal join and day-to-day use.

## UX Requirements
- Shop identity chrome in the programme workspace must stay compact and secondary to the programme task.
- Wallet readiness and support status must sit behind one quiet control rather than multiple top-level chips and buttons.
- The programme setup strip must be slim and top-aligned; it must not sit buried inside the main page body.
- Shop settings must stay available without competing visually with first-programme onboarding.
