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

require_file "docs/plans/active/F12-company-brand-surfaces.md"
require_file "docs/plans/completed/F11-programme-workspace-polish.md"
require_file "docs/specs/F12-company-brand-surfaces/REQUIREMENTS.md"
require_file "docs/specs/F12-company-brand-surfaces/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_file "src/Fic.Platform.Web/Components/Pages/Login.razor"
require_file "src/Fic.Platform.Web/Components/Pages/ForgotPassword.razor"
require_file "src/Fic.Platform.Web/Components/Pages/Consultancy.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SupportBilling.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SupportAccount.razor"
require_file "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"

require_text "F12 Company Brand Surfaces" "docs/plans/active/F12-company-brand-surfaces.md"
require_text "North Star Customer Solutions" "docs/specs/F12-company-brand-surfaces/REQUIREMENTS.md"
require_text "company-backed product entry point" "docs/specs/F12-company-brand-surfaces/ACCEPTANCE.md"
require_text "company-layer home, account, billing, and consultancy surfaces" "docs/ENGINEERING_HARNESS.md"
require_text "North Star Customer Solutions" "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_text "Training .*Consultancy" "src/Fic.Platform.Web/Components/Pages/Consultancy.razor"
require_text "North Star account support" "src/Fic.Platform.Web/Components/Pages/Login.razor"
require_text "Support by North Star" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "consultancy" "src/Fic.Platform.Web/Components/Layout/MainLayout.razor"
require_text "company-home" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "company-support" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "MerchantUtilityChrome_LinksToCompanySupportDestinations" "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F12 company brand surfaces validation passed."
