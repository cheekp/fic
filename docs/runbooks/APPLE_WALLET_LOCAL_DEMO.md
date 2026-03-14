# Apple Wallet Local Demo

This runbook gets the current FIC demo to the point where an iPhone can scan the join QR and add a signed Apple Wallet pass.

## What This Supports
- local LAN demo from your Mac to your iPhone
- signed `.pkpass` download from the join flow
- founder demo of merchant setup, QR join, and Apple Wallet add

## What This Does Not Yet Support
- Apple push update registration
- automatic pass refresh after `VisitAwarded`
- production secret storage or certificate rotation

## Apple Prerequisites

From Apple Developer, prepare:
- a Wallet pass type identifier
- a pass signing certificate for that pass type
- the exported signing certificate plus private key as a `.p12`
- the Apple WWDR intermediate certificate

Primary Apple references:
- [Creating the pass package](https://developer.apple.com/documentation/walletpasses/building-a-pass)
- [Images for Wallet passes](https://developer.apple.com/design/human-interface-guidelines/wallet#Passes)
- [Distributing Wallet passes from a website](https://developer.apple.com/documentation/walletpasses/distributing_and_updating_a_pass)

## Recommended Local File Layout

Keep certificate material outside the repo if possible. One workable layout is:

```text
~/fic-secrets/apple-wallet/
  fic-demo-signing.p12
  AppleWWDRCAG6.cer
```

## Configure Local Secrets

The web app now has a stable `UserSecretsId`, so use .NET user secrets instead of editing tracked config:

```bash
cd /Users/paulcheek/dev/fic
dotnet user-secrets set "Wallet:AppleWalletSigningConfigured" "true" --project src/Fic.Platform.Web/Fic.Platform.Web.csproj
dotnet user-secrets set "Wallet:AppleWallet:PassTypeIdentifier" "pass.com.yourteam.ficdemo" --project src/Fic.Platform.Web/Fic.Platform.Web.csproj
dotnet user-secrets set "Wallet:AppleWallet:TeamIdentifier" "ABCDE12345" --project src/Fic.Platform.Web/Fic.Platform.Web.csproj
dotnet user-secrets set "Wallet:AppleWallet:OrganizationName" "FIC Demo" --project src/Fic.Platform.Web/Fic.Platform.Web.csproj
dotnet user-secrets set "Wallet:AppleWallet:Description" "Coffee loyalty card" --project src/Fic.Platform.Web/Fic.Platform.Web.csproj
dotnet user-secrets set "Wallet:AppleWallet:P12CertificatePath" "/Users/yourname/fic-secrets/apple-wallet/fic-demo-signing.p12" --project src/Fic.Platform.Web/Fic.Platform.Web.csproj
dotnet user-secrets set "Wallet:AppleWallet:P12CertificatePassword" "your-p12-password" --project src/Fic.Platform.Web/Fic.Platform.Web.csproj
dotnet user-secrets set "Wallet:AppleWallet:WwdrCertificatePath" "/Users/yourname/fic-secrets/apple-wallet/AppleWWDRCAG6.cer" --project src/Fic.Platform.Web/Fic.Platform.Web.csproj
```

Environment variables also work if preferred:
- `Wallet__AppleWalletSigningConfigured`
- `Wallet__AppleWallet__PassTypeIdentifier`
- `Wallet__AppleWallet__TeamIdentifier`
- `Wallet__AppleWallet__OrganizationName`
- `Wallet__AppleWallet__Description`
- `Wallet__AppleWallet__P12CertificatePath`
- `Wallet__AppleWallet__P12CertificatePassword`
- `Wallet__AppleWallet__WwdrCertificatePath`

## Run The Demo

Start the LAN demo host:

```bash
cd /Users/paulcheek/dev/fic
./scripts/run-wallet-demo-lan.sh
```

The script prints the URL to open from your Mac. Use that LAN URL, not `localhost`, so the generated QR points back to your machine.
It also prints a Wallet demo readiness URL at `/support/wallet-demo`, which shows the current capability and any missing signing inputs.

## Demo Flow

1. On your Mac, open the printed `/portal/signup` URL.
2. Create a merchant and upload a PNG logo if you want the pass branding to carry through cleanly.
3. Open the merchant workspace.
4. Confirm the workspace shows `Signed Apple Wallet ready`, not `Preview fallback`.
5. Scan the join QR from your iPhone.
6. Tap `Add to Apple Wallet`.
7. Safari should download the `.pkpass` and hand off to Wallet.

## If It Falls Back To Preview

The environment is still missing one of:
- signing enabled flag
- pass type identifier
- team identifier
- `.p12` file path
- WWDR certificate path

The join page, merchant workspace, and `/support/wallet-demo` support page all surface the current capability message.

## If The iPhone Does Not Offer Wallet Add

Check these first:
- the join URL was generated from the LAN IP, not `localhost`
- the pass certificate matches the configured pass type identifier
- the `.p12` includes the private key
- the response is coming from Safari and not an embedded browser view

If local LAN delivery is still flaky, the next fallback is a public HTTPS dev host or tunnel for the same app build.
