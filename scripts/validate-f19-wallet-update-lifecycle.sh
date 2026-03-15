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

require_file "docs/plans/active/F19-wallet-update-lifecycle.md"
require_file "docs/plans/completed/F18-real-wallet-demo.md"
require_absent "docs/plans/active/F18-real-wallet-demo.md"
require_file "docs/specs/F19-wallet-update-lifecycle/REQUIREMENTS.md"
require_file "docs/specs/F19-wallet-update-lifecycle/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Platform.Web/Program.cs"
require_file "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_file "src/Fic.Platform.Web/Services/WalletPassWebServiceModels.cs"
require_file "src/Fic.WalletPasses/AppleWalletPassService.cs"
require_file "tests/Fic.Platform.Web.Tests/AppleWalletPassServiceTests.cs"
require_file "tests/Fic.Platform.Web.Tests/WalletPassWebServiceTests.cs"

require_text "F19 Wallet Update Lifecycle" "docs/plans/active/F19-wallet-update-lifecycle.md"
require_text "Apple Wallet pass update lifecycle" "docs/specs/F19-wallet-update-lifecycle/REQUIREMENTS.md"
require_text "updated serial endpoint" "docs/specs/F19-wallet-update-lifecycle/ACCEPTANCE.md"
require_text 'validator: `scripts/validate-f19-wallet-update-lifecycle.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "webServiceURL" "src/Fic.WalletPasses/AppleWalletPassService.cs"
require_text "authenticationToken" "src/Fic.WalletPasses/AppleWalletPassService.cs"
require_text "app.Map\\(\"/wallet/v1\"" "src/Fic.Platform.Web/Program.cs"
require_text "GetUpdatedWalletPassSerialNumbers" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "WalletWebService_RegistersUpdatedSerial_AndReturnsRefreshedPass" "tests/Fic.Platform.Web.Tests/WalletPassWebServiceTests.cs"
require_text "WalletWebService_UnregistersDeviceAndStopsReportingUpdates" "tests/Fic.Platform.Web.Tests/WalletPassWebServiceTests.cs"
require_text "CreatePackageAsync_BuildsSignedPassArchive_WhenCertificatesAreConfigured" "tests/Fic.Platform.Web.Tests/AppleWalletPassServiceTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F19 wallet update lifecycle validation passed."
