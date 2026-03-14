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

require_file "docs/plans/active/F14-entry-lane-focus.md"
require_file "docs/plans/completed/F13-entry-flow-polish.md"
require_file "docs/specs/F14-entry-lane-focus/REQUIREMENTS.md"
require_file "docs/specs/F14-entry-lane-focus/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_file "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"

require_text "F14 Entry Lane Focus" "docs/plans/active/F14-entry-lane-focus.md"
require_text "one clear primary product message" "docs/specs/F14-entry-lane-focus/REQUIREMENTS.md"
require_text "single-purpose and product-led" "docs/specs/F14-entry-lane-focus/ACCEPTANCE.md"
require_text "further reduction of narrative and competing signals" "docs/ENGINEERING_HARNESS.md"
require_text "Set up your wallet loyalty programme in minutes." "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_text "Backed by North Star|North Star supports" "src/Fic.Platform.Web/Components/Pages/Home.razor"
require_text "Create your shop account" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Billing is next, then your workspace opens." "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Final step before the workspace opens" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "company-home__signals" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "BillingPage_ReadsAsFinalOnboardingStep" "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

echo "F14 entry lane focus validation passed."
