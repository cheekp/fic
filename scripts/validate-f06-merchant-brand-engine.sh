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

require_file "docs/specs/F06-merchant-brand-engine/REQUIREMENTS.md"
require_file "docs/specs/F06-merchant-brand-engine/ACCEPTANCE.md"
require_file "src/Fic.MerchantAccounts/BrandThemes.cs"
require_file "src/Fic.Platform.Web/Services/MerchantBrandPresentationService.cs"
require_file "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_file "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_file "src/Fic.Platform.Web/Components/Pages/WalletCard.razor"
require_file "src/Fic.WalletPasses/AppleWalletPassService.cs"
require_file "src/Fic.Platform.Web/wwwroot/branding/palette.js"

require_text "MerchantBrandThemeCompiler" "src/Fic.MerchantAccounts/BrandThemes.cs"
require_text "LogoWidth" "src/Fic.Contracts/Snapshots.cs"
require_text "MerchantBrandPresentationService" "src/Fic.Platform.Web/Program.cs"
require_text "CurrentTheme.ThemeClass" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "signup-preview--studio" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "extractPaletteFromDataUrl" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "walletTheme" "src/Fic.Platform.Web/Components/Pages/WalletCard.razor"
require_text "MerchantBrandThemeCompiler.Compile" "src/Fic.WalletPasses/AppleWalletPassService.cs"
require_text "window.ficBranding.extractPaletteFromDataUrl" "src/Fic.Platform.Web/wwwroot/branding/palette.js"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F06 merchant brand engine validation passed."
