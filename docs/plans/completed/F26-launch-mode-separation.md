# F26 Launch Mode Separation

## Goal
- Make first-programme onboarding feel like one clear launch surface instead of normal workspace UI with extra setup panels layered into it.

## Scope
- Separate the zero-programme state into a dedicated full-width first-programme launch surface.
- Remove duplicate empty-state and guided-setup messaging from the Programmes area.
- Stop carrying launch state through normal workspace navigation.
- Keep shop chrome compact so programme setup is the clear focus.
- Use one slim setup strip at the top of Programmes when the merchant is still finishing the first programme.
- Return the merchant to normal programme workflow once the first programme has been configured.

## Out Of Scope
- New programme types or delivery channels.
- Broader public entry-lane work.
- Production billing or auth changes.

## Proof
- `scripts/validate-f26-launch-mode-separation.sh`
