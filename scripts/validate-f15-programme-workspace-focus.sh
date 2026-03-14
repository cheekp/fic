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

require_file "docs/plans/active/F15-programme-workspace-focus.md"
require_file "docs/plans/completed/F14-entry-lane-focus.md"
require_file "docs/specs/F15-programme-workspace-focus/REQUIREMENTS.md"
require_file "docs/specs/F15-programme-workspace-focus/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "F15 Programme Workspace Focus" "docs/plans/active/F15-programme-workspace-focus.md"
require_text "act as navigation, not as a content panel" "docs/specs/F15-programme-workspace-focus/REQUIREMENTS.md"
require_text "selected-programme header is compact and contextual" "docs/specs/F15-programme-workspace-focus/ACCEPTANCE.md"
require_text "programme workspace focus" "docs/ENGINEERING_HARNESS.md"
require_text "All programmes" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Current programme" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Current customer output" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "programme-context-bar" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "ProgrammesRail_ReadsAsNavigationNotExplainer" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ProgrammeContextBar_IsCompactAndDoesNotExplainTabs" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F15 programme workspace focus validation passed."
