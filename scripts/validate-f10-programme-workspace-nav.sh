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

require_file "docs/plans/active/F10-programme-workspace-nav.md"
require_file "docs/plans/completed/F09-merchant-workspace-polish.md"
require_file "docs/specs/F10-programme-workspace-nav/REQUIREMENTS.md"
require_file "docs/specs/F10-programme-workspace-nav/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_file "src/Fic.Platform.Web/Components/Layout/MainLayout.razor.css"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "F10 Programme Workspace Navigation" "docs/plans/active/F10-programme-workspace-nav.md"
require_text "merchant workspace hierarchy correction" "docs/ENGINEERING_HARNESS.md"
require_text "programme-centric workspace navigation" "docs/ENGINEERING_HARNESS.md"
require_text "Select a programme" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Programmes in this shop" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Customer delivery" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "wallet loyalty card" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Current output" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "New Programme" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Choose customer delivery" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Shop -> Programmes -> Operate" "docs/specs/F10-programme-workspace-nav/ACCEPTANCE.md"
require_text "workspace-detail-grid--programme-shell" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "workspace-detail-grid--programme-configure" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "delivery-option" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "programme-create-option" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "programme-group__header" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "programme-context-bar" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "merchant-shell-bar__utility" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor.css"
require_text "DefaultWorkspaceRoute_LandsInShopProgrammesOperate" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "PrimaryNavigation_DoesNotExposeEditShopAsPeerTab" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "Programmes_NewProgrammeFlow_MakesWalletDeliveryExplicit" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ProgrammesScope_GroupsProgrammesByLifecycle" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ProgrammeSelection_PreservesCurrentProgrammeSection" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F10 programme workspace navigation validation passed."
