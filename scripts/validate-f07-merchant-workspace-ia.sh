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

reject_text() {
  local pattern="$1"
  local path="$2"
  if rg -q "${pattern}" "${ROOT_DIR}/${path}"; then
    echo "unexpected pattern '${pattern}' found in ${path}" >&2
    exit 1
  fi
}

require_file "docs/specs/F07-merchant-workspace-ia/REQUIREMENTS.md"
require_file "docs/specs/F07-merchant-workspace-ia/ACCEPTANCE.md"
require_file "src/Fic.Platform.Web/Components/Pages/DevWorkspaces.razor"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_file "src/Fic.Platform.Web/Components/Pages/Login.razor"
require_file "src/Fic.Platform.Web/Components/Pages/ForgotPassword.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_file "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_file "src/Fic.Platform.Web/wwwroot/images/home-hero.jpeg"
require_file "src/Fic.Platform.Web/Components/Pages/Join.razor"
require_file "src/Fic.Platform.Web/Components/Pages/WalletCard.razor"
require_file "src/Fic.WalletPasses/AppleWalletPassService.cs"
require_file "tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj"
require_file "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"

require_text "@page \\\"/dev/workspaces\\\"" "src/Fic.Platform.Web/Components/Pages/DevWorkspaces.razor"
require_text "workspace-tabs__item" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Setup checklist" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Add Loyalty Card" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Begins on" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Expires on" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "Cards can only be joined and stamped between the begin and expiry dates." "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "UpdateMerchantBrandAsync" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "UpdateProgramme" "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "Expiry date must be on or after the begin date." "src/Fic.Platform.Web/Services/DemoPlatformState.cs"
require_text "Town or city" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Postcode" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Create your shop" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Create My Shop" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "@page \\\"/portal/signup/billing/\\{MerchantId:guid\\}\\\"" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "Continue to Workspace" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "Sign Up Now" "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_text "Forgot password\\?" "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_text "@page \\\"/account/login\\\"" "src/Fic.Platform.Web/Components/Pages/Login.razor"
require_text "@page \\\"/account/forgot-password\\\"" "src/Fic.Platform.Web/Components/Pages/ForgotPassword.razor"
require_text "merchant-shell-bar" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "Training &amp; Consultancy" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "Speak with Loyalty Agent" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "Open sales help" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "Get billing help" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "Billing" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "Log Out" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "ShowSidebar" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "Insights" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "This loyalty card becomes available on" "src/Fic.Platform.Web/Components/Pages/Join.razor"
require_text "Valid from" "src/Fic.Platform.Web/Components/Pages/WalletCard.razor"
require_text "EXPIRES" "src/Fic.WalletPasses/AppleWalletPassService.cs"
require_text "JoinCustomer_ReturnsNull_WhenProgrammeHasNotStarted" "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_text "AwardVisit_ReturnsNull_WhenProgrammeHasExpired" "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F07 merchant workspace IA validation passed."
