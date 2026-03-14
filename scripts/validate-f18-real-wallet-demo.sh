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

require_file "docs/plans/active/F18-real-wallet-demo.md"
require_file "docs/plans/completed/F17-workspace-visual-polish.md"
require_file "docs/specs/F18-real-wallet-demo/REQUIREMENTS.md"
require_file "docs/specs/F18-real-wallet-demo/ACCEPTANCE.md"
require_file "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Platform.Web/Components/Pages/SupportWalletDemo.razor"
require_file "src/Fic.WalletPasses/AppleWalletPassService.cs"
require_file "src/Fic.WalletPasses/WalletPassCapability.cs"
require_file "scripts/run-wallet-demo-lan.sh"
require_file "tests/Fic.Platform.Web.Tests/AppleWalletPassServiceTests.cs"
require_file "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

require_text "F18 Real Wallet Demo" "docs/plans/active/F18-real-wallet-demo.md"
require_text "signed \\.pkpass generation" "docs/specs/F18-real-wallet-demo/REQUIREMENTS.md"
require_text "Wallet demo support page" "docs/specs/F18-real-wallet-demo/ACCEPTANCE.md"
require_text "Wallet demo readiness" "docs/runbooks/APPLE_WALLET_LOCAL_DEMO.md"
require_text "real Apple Wallet founder-demo completion" "docs/ENGINEERING_HARNESS.md"
require_text "/support/wallet-demo" "src/Fic.Platform.Web/Components/Pages/SupportWalletDemo.razor"
require_text "DiagnosticItems" "src/Fic.WalletPasses/WalletPassCapability.cs"
require_text "Finish the Apple Wallet signing checklist" "src/Fic.WalletPasses/AppleWalletPassService.cs"
require_text "Wallet readiness page" "scripts/run-wallet-demo-lan.sh"
require_text "CreatePackageAsync_BuildsSignedPassArchive_WhenCertificatesAreConfigured" "tests/Fic.Platform.Web.Tests/AppleWalletPassServiceTests.cs"
require_text "Wallet demo readiness" "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"
require_text "Workspace_ShowsWalletDemoSetupLink_WhenSigningFallsBackToPreview" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F18 real wallet demo validation passed."
