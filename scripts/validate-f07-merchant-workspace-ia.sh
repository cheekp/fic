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

require_file "docs/specs/F07-merchant-workspace-ia/REQUIREMENTS.md"
require_file "docs/specs/F07-merchant-workspace-ia/ACCEPTANCE.md"
require_file "src/Fic.Platform.Web/Components/Pages/DevWorkspaces.razor"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_file "src/Fic.Platform.Web/Services/DemoPlatformState.cs"

require_text "@page \\\"/dev/workspaces\\\"" "src/Fic.Platform.Web/Components/Pages/DevWorkspaces.razor"
require_text "workspace-tabs__item" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "UpdateMerchantBrandAsync" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "UpdateProgramme" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "Set Up Your Loyalty Programme" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Continue to Your Loyalty Programme" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "What happens next" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "browse all local workspaces" "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_text "merchant-shell-bar" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "Billing" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "Log Out" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "ShowSidebar" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F07 merchant workspace IA validation passed."
