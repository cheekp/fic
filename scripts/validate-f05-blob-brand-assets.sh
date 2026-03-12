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

require_file "docs/specs/F05-blob-brand-assets/REQUIREMENTS.md"
require_file "docs/specs/F05-blob-brand-assets/ACCEPTANCE.md"
require_file "src/Fic.Platform.Web/Services/BlobMerchantBrandAssetStore.cs"
require_file "src/Fic.Platform.Web/Program.cs"
require_file "src/Fic.AppHost/AppHost.cs"
require_file "src/Fic.MerchantAccounts/BrandAssets.cs"
require_file "src/Fic.WalletPasses/AppleWalletPassService.cs"

require_text "UseBlobBrandAssets" "src/Fic.Platform.Web/Program.cs"
require_text "BlobMerchantBrandAssetStore" "src/Fic.Platform.Web/Program.cs"
require_text "MapGet\\(\"/merchant-brand-assets/" "src/Fic.Platform.Web/Program.cs"
require_text "RunAsEmulator" "src/Fic.AppHost/AppHost.cs"
require_text "AddBlobs\\(\"brandassets\"\\)" "src/Fic.AppHost/AppHost.cs"
require_text "GetAssetAsync" "src/Fic.MerchantAccounts/BrandAssets.cs"
require_text "brandAssetStore.GetAssetAsync" "src/Fic.WalletPasses/AppleWalletPassService.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F05 blob brand assets validation passed."
