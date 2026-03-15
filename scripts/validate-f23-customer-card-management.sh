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

require_file "docs/plans/active/F23-customer-card-management.md"
require_file "docs/plans/completed/F22-programme-model-and-templates.md"
require_absent "docs/plans/active/F22-programme-model-and-templates.md"
require_file "docs/specs/F23-customer-card-management/REQUIREMENTS.md"
require_file "docs/specs/F23-customer-card-management/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Contracts/CustomerCardStatus.cs"
require_file "src/Fic.Contracts/Snapshots.cs"
require_file "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "F23 Customer Card Management" "docs/plans/active/F23-customer-card-management.md"
require_text '`Customers` must show issued customer passes grouped by operational status' "docs/specs/F23-customer-card-management/REQUIREMENTS.md"
require_text 'validator: `scripts/validate-f23-customer-card-management.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "CustomerCardStatus" "src/Fic.Contracts/CustomerCardStatus.cs"
require_text "CustomerCardStatusLabel" "src/Fic.Contracts/Snapshots.cs"
require_text "CanRedeem" "src/Fic.Contracts/Snapshots.cs"
require_text "BuildCustomerCardStatus" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "BuildCustomerCardStatusLabel" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text ">Customers</a>" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Issued passes on this programme" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "customer-card-groups" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "ProgrammeNavigation_IncludesDedicatedCustomersSection" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ProgrammesCustomers_ShowsGroupedCustomerCardStatuses" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "WalletCardSnapshot_DerivesRewardReadyAndRedeemedStatuses" "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_text "WalletCardSnapshot_DerivesScheduledAndExpiredStatusesFromProgrammeWindow" "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F23 customer card management validation passed."
