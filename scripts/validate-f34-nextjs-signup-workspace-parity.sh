#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_CLI_HOME="${ROOT_DIR}/.dotnet_cli"
export NUGET_PACKAGES="${ROOT_DIR}/.nuget/packages"
export MSBuildEnableWorkloadResolver=false

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

require_file "docs/architecture/F34_NEXTJS_SIGNUP_WORKSPACE_PARITY_NOTE.md"
require_file "docs/rfcs/RFC-006-nextjs-signup-and-workspace-route-parity.md"
require_file "docs/plans/planned/F34-nextjs-signup-workspace-parity.md"
require_file "docs/specs/F34-nextjs-signup-workspace-parity/REQUIREMENTS.md"
require_file "docs/specs/F34-nextjs-signup-workspace-parity/ACCEPTANCE.md"

require_file "src/Fic.Platform.Frontend/app/portal/signup/plan/[merchantId]/page.tsx"
require_file "src/Fic.Platform.Frontend/app/portal/signup/billing/[merchantId]/page.tsx"
require_file "src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"
require_file "src/Fic.Platform.Frontend/lib/api.ts"
require_file "tests/Fic.Platform.Web.Tests/MerchantApiTests.cs"

require_text "createProgramme" "src/Fic.Platform.Frontend/lib/api.ts"
require_text "updateProgramme" "src/Fic.Platform.Frontend/lib/api.ts"
require_text "awardVisit" "src/Fic.Platform.Frontend/lib/api.ts"
require_text "redeemReward" "src/Fic.Platform.Frontend/lib/api.ts"
require_text "programmeSection" "src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"
require_text "ProgrammeMutationApis_SupportCreateConfigureOperateAndRedeemFlow" "tests/Fic.Platform.Web.Tests/MerchantApiTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
      --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false \
      --filter "FullyQualifiedName~MerchantApiTests|FullyQualifiedName~MerchantAuthBoundaryTests"

echo "F34 Next.js signup/workspace parity validation passed."
