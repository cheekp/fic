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

require_file "docs/plans/active/F25-demo-flow-hardening.md"
require_file "docs/plans/completed/F24-wallet-demo-critical-path.md"
require_absent "docs/plans/active/F24-wallet-demo-critical-path.md"
require_file "docs/specs/F25-demo-flow-hardening/REQUIREMENTS.md"
require_file "docs/specs/F25-demo-flow-hardening/ACCEPTANCE.md"
require_file "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "F25 Demo Flow Hardening" "docs/plans/active/F25-demo-flow-hardening.md"
require_text "signup-to-programme lane" "docs/specs/F25-demo-flow-hardening/REQUIREMENTS.md"
require_text 'validator: `scripts/validate-f25-demo-flow-hardening.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "first-programme setup" "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_text "Continue to first programme" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "launch=create" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "£19.99/mo" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "Guided setup" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Copy Join Link" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Save and open operate" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Continue to first programme" "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"
require_text "ProgrammesSection_LaunchCreateMode_ShowsGuidedSetupFlow" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ProgrammeConfigure_LaunchMode_SavesIntoOperate" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F25 demo flow hardening validation passed."
