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

require_file "docs/plans/active/F27-wallet-push-hardening.md"
require_file "docs/plans/completed/F26-launch-mode-separation.md"
require_absent "docs/plans/active/F26-launch-mode-separation.md"
require_file "docs/plans/active/README.md"
require_file "docs/specs/F27-wallet-push-hardening/REQUIREMENTS.md"
require_file "docs/specs/F27-wallet-push-hardening/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "README.md"
require_file "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_file "src/Fic.WalletPasses/AppleWalletPassUpdateNotifier.cs"
require_file "src/Fic.WalletPasses/WalletPassUpdateDispatchResult.cs"
require_file "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SupportWalletDemo.razor"
require_file "tests/Fic.Platform.Web.Tests/AppleWalletPassUpdateNotifierTests.cs"
require_file "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"

require_text "F27-wallet-push-hardening.md" "docs/plans/active/README.md"
require_text "F27 Wallet Push Hardening" "docs/plans/active/F27-wallet-push-hardening.md"
require_text "APNs dispatch responses must be classified" "docs/specs/F27-wallet-push-hardening/REQUIREMENTS.md"
require_text "retryable failure" "docs/specs/F27-wallet-push-hardening/ACCEPTANCE.md"
require_text 'validator: `scripts/validate-f27-wallet-push-hardening.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "Wallet push" "README.md"
require_text "If Refresh Did Not Arrive" "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_text "RetryableFailureCount" "src/Fic.WalletPasses/WalletPassUpdateDispatchResult.cs"
require_text "PermanentFailureCount" "src/Fic.WalletPasses/WalletPassUpdateDispatchResult.cs"
require_text "InvalidatedPushTokens" "src/Fic.WalletPasses/WalletPassUpdateDispatchResult.cs"
require_text "ClassifyFailure" "src/Fic.WalletPasses/AppleWalletPassUpdateNotifier.cs"
require_text "BadDeviceToken" "src/Fic.WalletPasses/AppleWalletPassUpdateNotifier.cs"
require_text "RemoveWalletPushTokens" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "refresh.HasInvalidatedPushTokens" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "retry needed" "src/Fic.Platform.Web/Components/Pages/SupportWalletDemo.razor"
require_text "NotifyPassUpdatedAsync_ClassifiesRetryableAndPermanentTokenFailures" "tests/Fic.Platform.Web.Tests/AppleWalletPassUpdateNotifierTests.cs"
require_text "RemoveWalletPushTokens_RemovesOnlyMatchingTokensForSelectedCard" "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
        --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false \
        --filter "FullyQualifiedName~AppleWalletPassUpdateNotifierTests|FullyQualifiedName~DemoPlatformStateTests"

echo "F27 wallet push hardening validation passed."
