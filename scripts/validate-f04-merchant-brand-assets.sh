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

require_file "docs/specs/F04-merchant-brand-assets/REQUIREMENTS.md"
require_file "docs/specs/F04-merchant-brand-assets/ACCEPTANCE.md"
require_file "src/Fic.MerchantAccounts/BrandAssets.cs"
require_file "src/Fic.Platform.Web/Services/LocalMerchantBrandAssetStore.cs"
require_file "src/Fic.Platform.Web/Program.cs"
require_file "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_file "src/Fic.WalletPasses/AppleWalletPassService.cs"

require_text "IMerchantBrandAssetStore" "src/Fic.MerchantAccounts/BrandAssets.cs"
require_text "LocalMerchantBrandAssetStore" "src/Fic.Platform.Web/Services/LocalMerchantBrandAssetStore.cs"
require_text "/merchant-brand-assets" "src/Fic.Platform.Web/Program.cs"
require_text "image/png" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "TryLoadStoredPng" "src/Fic.WalletPasses/AppleWalletPassService.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build src/Fic.Platform.Web/Fic.Platform.Web.csproj --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F04 merchant brand assets validation passed."
