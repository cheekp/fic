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

require_file "docs/plans/active/F21-journey-ia-reset.md"
require_file "docs/plans/completed/F20-merchant-auth-boundary.md"
require_absent "docs/plans/active/F20-merchant-auth-boundary.md"
require_file "docs/specs/F21-journey-ia-reset/REQUIREMENTS.md"
require_file "docs/specs/F21-journey-ia-reset/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Contracts/ProgrammeTemplates.cs"
require_file "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_file "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "F21 Journey IA Reset" "docs/plans/active/F21-journey-ia-reset.md"
require_text "must not silently create a starter programme" "docs/specs/F21-journey-ia-reset/REQUIREMENTS.md"
require_text "first-programme setup experience" "docs/specs/F21-journey-ia-reset/ACCEPTANCE.md"
require_text 'validator: `scripts/validate-f21-journey-ia-reset.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "ProgrammeTemplateOption" "src/Fic.Contracts/ProgrammeTemplates.cs"
require_text "coffee-food-offer" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "createStarterProgramme = false" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "CreateProgramme\\(Guid merchantId, string templateKey, string baseUri\\)" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "Step 1 of 3" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Step 2 of 3" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "Create your first programme" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Coffee plus food offer" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "CreateMerchantAsync_CreatesShopWithoutProgrammeUntilProgrammeIsConfigured" "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_text "ProgrammesSection_ShowsFirstProgrammeSetup_WhenShopHasNoProgrammes" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F21 journey IA reset validation passed."
