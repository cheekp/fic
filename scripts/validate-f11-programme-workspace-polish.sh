#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_CLI_HOME="${ROOT_DIR}/.dotnet_cli"
export NUGET_PACKAGES="${ROOT_DIR}/.nuget/packages"
export MSBuildEnableWorkloadResolver=false

TEST_NUGET_PACKAGES="${NUGET_PACKAGES}"
if [[ ! -d "${TEST_NUGET_PACKAGES}/microsoft.net.test.sdk" && -d "${HOME}/.nuget/packages/microsoft.net.test.sdk" ]]; then
  TEST_NUGET_PACKAGES="${HOME}/.nuget/packages"
fi

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

require_file "docs/plans/active/F11-programme-workspace-polish.md"
require_file "docs/plans/completed/F10-programme-workspace-nav.md"
require_file "docs/specs/F11-programme-workspace-polish/REQUIREMENTS.md"
require_file "docs/specs/F11-programme-workspace-polish/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "README.md"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "F11 Programme Workspace Polish" "docs/plans/active/F11-programme-workspace-polish.md"
require_text "F10" "docs/plans/completed/F10-programme-workspace-nav.md"
require_text "programme workspace polish and daily-use reduction of visual density" "docs/ENGINEERING_HARNESS.md"
require_text "shop settings available as an in-context merchant workspace surface" "docs/ENGINEERING_HARNESS.md"
require_text "in-context surface" "docs/specs/F11-programme-workspace-polish/ACCEPTANCE.md"
require_text "Shop settings drawer" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "settings" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Choose customer delivery" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "workspace-drawer-backdrop" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "workspace-drawer" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "programme-rail" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "LegacyEditRoute_OpensShopSettingsDrawer" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ShopSettingsDrawer_CanOpenFromProgrammesWithoutLeavingProgrammeSurface" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F11 programme workspace polish validation passed."
