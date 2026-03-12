#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_CLI_HOME="${ROOT_DIR}/.dotnet_cli"
export NUGET_PACKAGES="${ROOT_DIR}/.nuget/packages"
export MSBuildEnableWorkloadResolver=false

require_file() {
  local path="$1"
  if [[ ! -f "${ROOT_DIR}/${path}" ]]; then
    echo "missing file: ${path}" >&2
    exit 1
  fi
}

require_text() {
  local pattern="$1"
  local path="$2"
  if ! rg -q "${pattern}" "${ROOT_DIR}/${path}"; then
    echo "missing pattern '${pattern}' in ${path}" >&2
    exit 1
  fi
}

require_file "docs/specs/F02-apple-wallet-pass/REQUIREMENTS.md"
require_file "docs/specs/F02-apple-wallet-pass/ACCEPTANCE.md"
require_file "docs/specs/F02-apple-wallet-pass/contracts/APPLE_WALLET_PASS_CONTRACT.md"
require_file "Fic.sln"

require_text "pkpass" "docs/specs/F02-apple-wallet-pass/REQUIREMENTS.md"
require_text "preview fallback" "docs/specs/F02-apple-wallet-pass/REQUIREMENTS.md"
require_text "barcode message" "docs/specs/F02-apple-wallet-pass/contracts/APPLE_WALLET_PASS_CONTRACT.md"
require_text "Wallet:AppleWalletSigningConfigured" "docs/specs/F02-apple-wallet-pass/REQUIREMENTS.md"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false "${ROOT_DIR}/Fic.sln"

echo "F02 Apple Wallet pass validation passed."
