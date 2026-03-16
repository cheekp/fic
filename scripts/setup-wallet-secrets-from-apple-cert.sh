#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_PATH="${ROOT_DIR}/src/Fic.Platform.Web/Fic.Platform.Web.csproj"
WALLET_DIR="${HOME}/fic-secrets/apple-wallet"
KEY_PATH="${WALLET_DIR}/fic-pass-type-id.key"
CSR_PATH="${WALLET_DIR}/fic-pass-type-id.csr"
WWDR_PATH="${WALLET_DIR}/AppleWWDRCAG6.cer"
P12_PATH="${WALLET_DIR}/fic-demo-signing.p12"
CERT_COPY_PATH="${WALLET_DIR}/pass-type-id.cer"

PASS_TYPE_ID=""
TEAM_ID=""
CERT_PATH=""
P12_PASSWORD=""
ORG_NAME="FIC Demo"
DESCRIPTION="Coffee loyalty card"

usage() {
  cat <<USAGE
Usage:
  ./scripts/setup-wallet-secrets-from-apple-cert.sh \\
    --cert /path/to/pass-type-id.cer \\
    --p12-password '<p12-password>' \\
    [--pass-type-id pass.com.yourteam.ficdemo] \\
    [--team-id ABCDE12345] \\
    [--org-name 'FIC Demo'] \\
    [--description 'Coffee loyalty card']

Notes:
- If --pass-type-id or --team-id are omitted, the script tries to infer them from the certificate subject.
- Requires local key from CSR step at: ${KEY_PATH}
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --cert)
      CERT_PATH="$2"
      shift 2
      ;;
    --p12-password)
      P12_PASSWORD="$2"
      shift 2
      ;;
    --pass-type-id)
      PASS_TYPE_ID="$2"
      shift 2
      ;;
    --team-id)
      TEAM_ID="$2"
      shift 2
      ;;
    --org-name)
      ORG_NAME="$2"
      shift 2
      ;;
    --description)
      DESCRIPTION="$2"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ -z "${CERT_PATH}" || -z "${P12_PASSWORD}" ]]; then
  echo "--cert and --p12-password are required." >&2
  usage
  exit 1
fi

if [[ ! -f "${CERT_PATH}" ]]; then
  echo "Certificate not found: ${CERT_PATH}" >&2
  exit 1
fi

if [[ ! -f "${KEY_PATH}" ]]; then
  echo "Missing private key at ${KEY_PATH}" >&2
  echo "Generate CSR first (already prepared in most cases): ${CSR_PATH}" >&2
  exit 1
fi

if [[ ! -f "${WWDR_PATH}" ]]; then
  echo "Missing WWDR cert at ${WWDR_PATH}" >&2
  exit 1
fi

mkdir -p "${WALLET_DIR}"
cp -f "${CERT_PATH}" "${CERT_COPY_PATH}"

subject_line=""
if subject_line="$(openssl x509 -in "${CERT_COPY_PATH}" -inform DER -noout -subject 2>/dev/null)"; then
  :
elif subject_line="$(openssl x509 -in "${CERT_COPY_PATH}" -noout -subject 2>/dev/null)"; then
  :
else
  echo "Could not parse certificate subject from ${CERT_COPY_PATH}" >&2
  exit 1
fi

if [[ -z "${PASS_TYPE_ID}" ]]; then
  if [[ "${subject_line}" =~ UID[[:space:]]*=[[:space:]]*([^,]+) ]]; then
    PASS_TYPE_ID="${BASH_REMATCH[1]}"
  fi
fi

if [[ -z "${TEAM_ID}" ]]; then
  if [[ "${subject_line}" =~ OU[[:space:]]*=[[:space:]]*([^,]+) ]]; then
    TEAM_ID="${BASH_REMATCH[1]}"
  fi
fi

PASS_TYPE_ID="$(echo "${PASS_TYPE_ID}" | xargs)"
TEAM_ID="$(echo "${TEAM_ID}" | xargs)"

if [[ -z "${PASS_TYPE_ID}" ]]; then
  echo "Could not infer pass type identifier from cert. Provide --pass-type-id." >&2
  echo "Subject: ${subject_line}" >&2
  exit 1
fi

if [[ -z "${TEAM_ID}" ]]; then
  echo "Could not infer team identifier from cert. Provide --team-id." >&2
  echo "Subject: ${subject_line}" >&2
  exit 1
fi

openssl pkcs12 -export \
  -inkey "${KEY_PATH}" \
  -in "${CERT_COPY_PATH}" \
  -certfile "${WWDR_PATH}" \
  -out "${P12_PATH}" \
  -passout "pass:${P12_PASSWORD}" >/dev/null 2>&1

dotnet user-secrets set "Wallet:AppleWalletSigningConfigured" "true" --project "${PROJECT_PATH}" >/dev/null
dotnet user-secrets set "Wallet:AppleWallet:PushNotificationsEnabled" "true" --project "${PROJECT_PATH}" >/dev/null
dotnet user-secrets set "Wallet:AppleWallet:PassTypeIdentifier" "${PASS_TYPE_ID}" --project "${PROJECT_PATH}" >/dev/null
dotnet user-secrets set "Wallet:AppleWallet:TeamIdentifier" "${TEAM_ID}" --project "${PROJECT_PATH}" >/dev/null
dotnet user-secrets set "Wallet:AppleWallet:OrganizationName" "${ORG_NAME}" --project "${PROJECT_PATH}" >/dev/null
dotnet user-secrets set "Wallet:AppleWallet:Description" "${DESCRIPTION}" --project "${PROJECT_PATH}" >/dev/null
dotnet user-secrets set "Wallet:AppleWallet:P12CertificatePath" "${P12_PATH}" --project "${PROJECT_PATH}" >/dev/null
dotnet user-secrets set "Wallet:AppleWallet:P12CertificatePassword" "${P12_PASSWORD}" --project "${PROJECT_PATH}" >/dev/null
dotnet user-secrets set "Wallet:AppleWallet:WwdrCertificatePath" "${WWDR_PATH}" --project "${PROJECT_PATH}" >/dev/null
dotnet user-secrets set "Wallet:AppleWallet:UseSandboxPushEndpoint" "true" --project "${PROJECT_PATH}" >/dev/null

echo "Configured Wallet secrets for ${PROJECT_PATH}"
echo "Pass type identifier: ${PASS_TYPE_ID}"
echo "Team identifier: ${TEAM_ID}"
echo "P12 path: ${P12_PATH}"
echo "WWDR path: ${WWDR_PATH}"

echo "\nNext:" 
echo "1) ./scripts/run-wallet-demo-lan.sh"
echo "2) Open /support/wallet-demo and confirm both issuance and refresh are Ready"
