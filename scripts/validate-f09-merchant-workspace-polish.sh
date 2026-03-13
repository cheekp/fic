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

require_file "docs/plans/active/F09-merchant-workspace-polish.md"
require_file "docs/plans/completed/F08-workspace-polish-and-bunit.md"
require_file "docs/specs/F09-merchant-workspace-polish/REQUIREMENTS.md"
require_file "docs/specs/F09-merchant-workspace-polish/ACCEPTANCE.md"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "Onboarding roadmap" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Hide setup" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Dismiss" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Merchant-owned root context" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Select the programme you want to run" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Daily use for this programme" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Programme configuration" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Shop overview" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Programmes" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "setup-roadmap" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "shop-profile-summary" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "workspace-tabs--sub" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "ShopOverview_ShowsCollapsedRoadmapByDefault_WhenSetupIsComplete" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ShopEdit_ShowsShopEditorAndNotProgrammeOperations" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ProgrammesOperate_UsesSelectedProgrammeContext" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ProgrammesInsights_ShowsSelectedProgrammeMetrics" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F09 merchant workspace polish validation passed."
