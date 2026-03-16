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

require_file "docs/plans/active/F30-onboarding-surface-polish-and-completion-handoff.md"
require_file "docs/plans/completed/F29-onboarding-guardrails-and-tiered-billing.md"
require_file "docs/specs/F30-onboarding-surface-polish-and-completion-handoff/REQUIREMENTS.md"
require_file "docs/specs/F30-onboarding-surface-polish-and-completion-handoff/ACCEPTANCE.md"
require_file "docs/runbooks/UX_QA_PLAYBOOK.md"
require_file "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_file "src/Fic.Platform.Web/Components/Layout/MainLayout.razor.css"
require_file "src/Fic.Platform.Web/wwwroot/images/apple-pay-mark.svg"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_file "tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs"

require_text "F30-onboarding-surface-polish-and-completion-handoff.md" "docs/plans/active/README.md"
require_text "Polish onboarding surfaces" "README.md"
require_text 'validator: `scripts/validate-f30-onboarding-surface-polish.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "Sign up now" "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_text "apple-pay-mark.svg" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "Card number" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "Setup complete" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "DismissSetupCompletionNotice" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "merchant-shell-bar__menu" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "justify-content: flex-start" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor.css"
require_text "company-home--focused" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "billing-method-option" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "setup-complete-banner" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "apple-pay-mark.svg" "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"
require_text "Open insights" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "scripts/validate-ux-surface.sh" "docs/runbooks/UX_QA_PLAYBOOK.md"
require_text "UxQualityGateTests" "tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
      --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false \
      --filter "FullyQualifiedName~CompanyBrandSurfaceTests|FullyQualifiedName~VendorWorkspaceComponentTests|FullyQualifiedName~MerchantAuthBoundaryTests|FullyQualifiedName~UxQualityGateTests"

echo "F30 onboarding surface polish validation passed."
