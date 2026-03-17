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

require_file "tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "docs/runbooks/UX_QA_PLAYBOOK.md"
require_file "scripts/validate-css-budget.sh"

require_text "FIC_UX_BROWSER_SMOKE" "tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs"
require_text "programme-context-bar__descriptor" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "@media \\(prefers-reduced-motion: reduce\\)" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "scripts/validate-ux-surface.sh" "docs/runbooks/UX_QA_PLAYBOOK.md"
require_text "scripts/validate-css-budget.sh" "docs/runbooks/UX_QA_PLAYBOOK.md"

"${ROOT_DIR}/scripts/validate-css-budget.sh"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
      --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false \
      --filter "FullyQualifiedName~UxQualityGateTests"

if [[ "${FIC_UX_BROWSER_SMOKE:-0}" == "1" ]]; then
  env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
      NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
      MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
      dotnet build "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
        --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false >/dev/null

  PLAYWRIGHT_INSTALLER_SH="${ROOT_DIR}/tests/Fic.Platform.Web.Tests/bin/Debug/net10.0/playwright.sh"
  PLAYWRIGHT_INSTALLER_PS1="${ROOT_DIR}/tests/Fic.Platform.Web.Tests/bin/Debug/net10.0/playwright.ps1"
  if [[ -x "${PLAYWRIGHT_INSTALLER_SH}" ]]; then
    "${PLAYWRIGHT_INSTALLER_SH}" install chromium
  elif [[ -f "${PLAYWRIGHT_INSTALLER_PS1}" ]]; then
    pwsh "${PLAYWRIGHT_INSTALLER_PS1}" install chromium
  fi

  env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
      NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
      MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
      FIC_UX_BROWSER_SMOKE=1 \
      dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
        --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false \
        --filter "FullyQualifiedName~UxBrowserSmokeTests"
else
  echo "Skipping optional browser smoke. Set FIC_UX_BROWSER_SMOKE=1 to run Playwright overflow checks."
fi

echo "UX surface validation passed."
