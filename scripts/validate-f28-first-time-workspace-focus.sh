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

require_file "docs/plans/active/F28-first-time-workspace-focus.md"
require_file "docs/plans/completed/F27-wallet-push-hardening.md"
require_absent "docs/plans/active/F27-wallet-push-hardening.md"
require_file "docs/plans/active/README.md"
require_file "docs/specs/F28-first-time-workspace-focus/REQUIREMENTS.md"
require_file "docs/specs/F28-first-time-workspace-focus/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "README.md"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "F28-first-time-workspace-focus.md" "docs/plans/active/README.md"
require_text "F28 First-Time Workspace Focus" "docs/plans/active/F28-first-time-workspace-focus.md"
require_text "strict first-time route gating" "docs/specs/F28-first-time-workspace-focus/REQUIREMENTS.md"
require_text "cannot remain on deep links" "docs/specs/F28-first-time-workspace-focus/ACCEPTANCE.md"
require_text 'validator: `scripts/validate-f28-first-time-workspace-focus.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "first-time merchant workflow" "README.md"
require_text "IsFirstTimeMode" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "EnforceFirstTimeRouteGuard" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "FirstTimeRouteGuard_RedirectsShopInsightsToProgrammesOperate" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ProgrammeNavigation_HidesAdvancedSections_InFirstTimeMode" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "Workspace_HidesWalletDemoSetupLink_InFirstTimeMode" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
        --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false \
        --filter "FullyQualifiedName~VendorWorkspaceComponentTests"

echo "F28 first-time workspace focus validation passed."
