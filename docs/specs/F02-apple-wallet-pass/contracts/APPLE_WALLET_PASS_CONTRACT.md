# Apple Wallet Pass Contract

## Purpose

Define the minimum contract surface for real Apple Wallet issuance in F02.

## Runtime Modes

- `preview`
- `apple-wallet-pass`

## Minimum Signing Configuration

- `signingConfigured`
- `passTypeIdentifier`
- `teamIdentifier`
- `organizationName`
- `description`
- `p12CertificatePath`
- `p12CertificatePassword`
- `wwdrCertificatePath`

## Minimum Pass Fields

- `formatVersion`
- `passTypeIdentifier`
- `serialNumber`
- `teamIdentifier`
- `organizationName`
- `description`
- `logoText`
- `backgroundColor`
- `foregroundColor`
- `labelColor`
- `barcode.message`
- `barcode.format`
- `barcode.messageEncoding`
- `storeCard.primaryFields`
- `storeCard.secondaryFields`
- `storeCard.auxiliaryFields`

## Package Files

- `pass.json`
- `manifest.json`
- `signature`
- `icon.png`
- `icon@2x.png`

## Invariants

- the pass serial number must map back to the platform wallet-card identity
- the pass barcode message must equal the vendor scan code for the same card
- the pass progress text must be derived from the same projection fields used by the vendor workspace
- when signing is unavailable, the system must fall back to preview mode instead of offering a broken download
