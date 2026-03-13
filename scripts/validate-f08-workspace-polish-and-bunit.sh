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

require_file "docs/plans/active/F08-workspace-polish-and-bunit.md"
require_file "docs/specs/F08-workspace-polish-and-bunit/REQUIREMENTS.md"
require_file "docs/specs/F08-workspace-polish-and-bunit/ACCEPTANCE.md"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj"
require_file "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "PackageReference Include=\"bunit\"" "tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj"
require_text "BunitContext" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "context\\.Render<VendorWorkspace>" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "NavigateToWorkspace" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ShopTab_ShowsSetupChecklist" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "CardsTab_DoesNotShowCustomerJoinAction" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "CustomersTab_DisablesJoinAction_WhenProgrammeIsScheduled" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "CustomersTab_UsesSelectedProgrammeContext" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "Setup checklist" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Add Loyalty Card" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Open Customer Join" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Card specific" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "GetProgrammeUrl" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F08 workspace polish and bUnit validation passed."
