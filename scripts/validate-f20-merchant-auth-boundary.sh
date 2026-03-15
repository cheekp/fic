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

require_file "docs/plans/active/F20-merchant-auth-boundary.md"
require_file "docs/plans/completed/F19-wallet-update-lifecycle.md"
require_absent "docs/plans/active/F19-wallet-update-lifecycle.md"
require_file "docs/specs/F20-merchant-auth-boundary/REQUIREMENTS.md"
require_file "docs/specs/F20-merchant-auth-boundary/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.MerchantAccounts/MerchantPasswordHasher.cs"
require_file "src/Fic.Platform.Web/Program.cs"
require_file "src/Fic.Platform.Web/Services/MerchantAccessModels.cs"
require_file "src/Fic.Platform.Web/Services/MerchantSessionClaims.cs"
require_file "src/Fic.Platform.Web/Components/Pages/Login.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_file "src/Fic.Platform.Web/Components/Pages/AccessDenied.razor"
require_file "tests/Fic.Platform.Web.Tests/MerchantAuthBoundaryTests.cs"
require_file "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"

require_text "F20 Merchant Auth Boundary" "docs/plans/active/F20-merchant-auth-boundary.md"
require_text "cookie-backed merchant session" "docs/specs/F20-merchant-auth-boundary/REQUIREMENTS.md"
require_text "signed-in merchant cannot open another merchant's workspace" "docs/specs/F20-merchant-auth-boundary/ACCEPTANCE.md"
require_text 'validator: `scripts/validate-f20-merchant-auth-boundary.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "AddAuthentication" "src/Fic.Platform.Web/Program.cs"
require_text "/account/session/login" "src/Fic.Platform.Web/Program.cs"
require_text "/account/session/complete-signup" "src/Fic.Platform.Web/Program.cs"
require_text "/account/logout" "src/Fic.Platform.Web/Program.cs"
require_text "TryGetMerchantRouteId" "src/Fic.Platform.Web/Program.cs"
require_text "MerchantPasswordHasher" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "Owner password" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "You do not have access to that workspace" "src/Fic.Platform.Web/Components/Pages/AccessDenied.razor"
require_text "MerchantWorkspace_RedirectsUnauthenticatedUser_ToLogin" "tests/Fic.Platform.Web.Tests/MerchantAuthBoundaryTests.cs"
require_text "ConfigureMerchantAccess_StoresCredentials_AndAllowsAuthentication" "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F20 merchant auth boundary validation passed."
