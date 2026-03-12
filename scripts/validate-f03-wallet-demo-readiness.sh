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

require_file "docs/specs/F03-wallet-demo-readiness/REQUIREMENTS.md"
require_file "docs/specs/F03-wallet-demo-readiness/ACCEPTANCE.md"
require_file "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_file "scripts/run-wallet-demo-lan.sh"
require_file "src/Fic.Platform.Web/Fic.Platform.Web.csproj"
require_file "src/Fic.Platform.Web/wwwroot/wallet-assets/icon.png"
require_file "src/Fic.Platform.Web/wwwroot/wallet-assets/icon@2x.png"
require_file "src/Fic.Platform.Web/wwwroot/wallet-assets/icon@3x.png"

require_text "UserSecretsId" "src/Fic.Platform.Web/Fic.Platform.Web.csproj"
require_text "Signed Apple Wallet ready" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Preview fallback" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Download Wallet Pass" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "dotnet user-secrets set" "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build src/Fic.Platform.Web/Fic.Platform.Web.csproj --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F03 wallet demo readiness validation passed."
