# F28 First-Time Workspace Focus

## Goal
- Make the merchant workspace feel obvious for first-time users by keeping only the minimum actions needed to get the first customer join and first stamp done.

## Scope
- Add strict first-time route gating so deep links cannot drop new merchants into secondary sections before first join.
- Keep first-time navigation focused on `Programmes` with only `Operate` and `Configure` visible.
- Remove secondary first-time clutter from `Operate` and replace it with one clear next-step panel.
- Re-enable advanced programme surfaces only after first real customer join is present.
- Keep shop settings accessible without exposing full secondary workspace lanes.

## Out Of Scope
- Broad visual redesign of mature (post-first-join) workspace surfaces.
- New loyalty features or delivery channels.
- Changes to account, billing, or auth boundaries.

## Proof
- `scripts/validate-f28-first-time-workspace-focus.sh`
