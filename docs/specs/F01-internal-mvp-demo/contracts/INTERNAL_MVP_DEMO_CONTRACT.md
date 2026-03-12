# Internal MVP Demo Contract

## Purpose

Define the minimum shared contract surface for the first internal MVP/demo slice.

## Programme Configuration

Minimum programme configuration fields:

- `vendorDisplayName`
- `rewardItemLabel`
- `rewardThreshold`
- `rewardCopy`
- `logoAssetRef`
- `primaryColor`
- `accentColor`

## Customer Progress Projection

Minimum progress projection fields:

- `vendorId`
- `programmeId`
- `customerCardId`
- `currentCount`
- `targetCount`
- `progressDisplayText`
- `rewardState`
- `walletPassId`
- `lastUpdatedUtc`

## Invariants

- `progressDisplayText` must be derivable from `currentCount`, `targetCount`, and programme wording.
- vendor PWA and wallet rendering must use the same projection fields for progress state.
- wallet state must never lead the underlying projection.
