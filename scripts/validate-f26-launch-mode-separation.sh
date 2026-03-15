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

require_absent() {
  local path="$1"
  if [[ -e "${ROOT_DIR}/${path}" ]]; then
    echo "unexpected file present: ${path}" >&2
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

require_absent_text() {
  local pattern="$1"
  local path="$2"
  if rg -q "${pattern}" "${ROOT_DIR}/${path}"; then
    echo "unexpected pattern '${pattern}' found in ${path}" >&2
    exit 1
  fi
}

require_file "docs/plans/active/F26-launch-mode-separation.md"
require_file "docs/plans/completed/F25-demo-flow-hardening.md"
require_absent "docs/plans/active/F25-demo-flow-hardening.md"
require_file "docs/specs/F26-launch-mode-separation/REQUIREMENTS.md"
require_file "docs/specs/F26-launch-mode-separation/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_file "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"

require_text "F26 Launch Mode Separation" "docs/plans/active/F26-launch-mode-separation.md"
require_text "dedicated launch flow" "docs/specs/F26-launch-mode-separation/REQUIREMENTS.md"
require_text 'validator: `scripts/validate-f26-launch-mode-separation.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "launch-mode separation" "docs/ENGINEERING_HARNESS.md"
require_text "launch=create" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "workspace-detail-grid--single-pane" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "First programme setup" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Save and open operate" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "ProgrammesSection_ShowsFirstProgrammeSetup_WhenShopHasNoProgrammes" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ProgrammeConfigure_FirstProgrammeSetup_SavesIntoOperateWithoutLaunchState" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_absent_text "launch=configure" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_absent_text "launch=operate" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "launch=create" "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F26 launch mode separation validation passed."
