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

require_file "docs/plans/active/F22-programme-model-and-templates.md"
require_file "docs/plans/completed/F21-journey-ia-reset.md"
require_absent "docs/plans/active/F21-journey-ia-reset.md"
require_file "docs/specs/F22-programme-model-and-templates/REQUIREMENTS.md"
require_file "docs/specs/F22-programme-model-and-templates/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Contracts/ProgrammeTemplates.cs"
require_file "src/Fic.Contracts/Snapshots.cs"
require_file "src/Fic.LoyaltyCore/LoyaltyProgramme.cs"
require_file "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "F22 Programme Model And Templates" "docs/plans/active/F22-programme-model-and-templates.md"
require_text "programme type" "docs/specs/F22-programme-model-and-templates/REQUIREMENTS.md"
require_text "current customer delivery output" "docs/specs/F22-programme-model-and-templates/ACCEPTANCE.md"
require_text 'validator: `scripts/validate-f22-programme-model-and-templates.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "ProgrammeTypeKey" "src/Fic.Contracts/ProgrammeTemplates.cs"
require_text "DeliveryTypeLabel" "src/Fic.Contracts/ProgrammeTemplates.cs"
require_text "OutputLabel" "src/Fic.Contracts/Snapshots.cs"
require_text "ProgrammeTypeLabel" "src/Fic.LoyaltyCore/LoyaltyProgramme.cs"
require_text "Apple Wallet pass" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "Wallet offer card" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "Programme model" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Current output" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "CreateProgramme_FirstProgrammeUsesTemplateAndOneYearWindow" "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_text "Programmes_NewProgrammeFlow_MakesWalletDeliveryExplicit" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F22 programme model and templates validation passed."
