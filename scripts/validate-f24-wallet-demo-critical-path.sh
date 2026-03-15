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

require_file "docs/plans/active/F24-wallet-demo-critical-path.md"
require_file "docs/plans/completed/F23-customer-card-management.md"
require_absent "docs/plans/active/F23-customer-card-management.md"
require_file "docs/specs/F24-wallet-demo-critical-path/REQUIREMENTS.md"
require_file "docs/specs/F24-wallet-demo-critical-path/ACCEPTANCE.md"
require_file "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.WalletPasses/IWalletPassUpdateNotifier.cs"
require_file "src/Fic.WalletPasses/AppleWalletPassUpdateNotifier.cs"
require_file "src/Fic.WalletPasses/WalletPassPushCapability.cs"
require_file "src/Fic.WalletPasses/WalletPassUpdateDispatchResult.cs"
require_file "src/Fic.WalletPasses/AppleWalletCertificateLoader.cs"
require_file "src/Fic.Platform.Web/Components/Pages/SupportWalletDemo.razor"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_file "tests/Fic.Platform.Web.Tests/AppleWalletPassUpdateNotifierTests.cs"
require_file "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"

require_text "F24 Wallet Demo Critical Path" "docs/plans/active/F24-wallet-demo-critical-path.md"
require_text "Wallet founder demo" "docs/specs/F24-wallet-demo-critical-path/REQUIREMENTS.md"
require_text 'validator: `scripts/validate-f24-wallet-demo-critical-path.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "PushNotificationsEnabled" "src/Fic.WalletPasses/AppleWalletPassOptions.cs"
require_text "NotifyPassUpdatedAsync" "src/Fic.WalletPasses/IWalletPassUpdateNotifier.cs"
require_text "Wallet refresh requests can be sent" "src/Fic.WalletPasses/AppleWalletPassUpdateNotifier.cs"
require_text "GetWalletPushTokens" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "Wallet refresh ready" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Pass refresh" "src/Fic.Platform.Web/Components/Pages/SupportWalletDemo.razor"
require_text "PushNotificationsEnabled" "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_text "Apple: Updating a pass" "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_text "NotifyPassUpdatedAsync_SendsTopicToConfiguredEndpoint" "tests/Fic.Platform.Web.Tests/AppleWalletPassUpdateNotifierTests.cs"
require_text "Pass refresh" "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F24 wallet demo critical path validation passed."
