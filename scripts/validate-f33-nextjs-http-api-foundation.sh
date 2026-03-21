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

require_file "docs/architecture/F33_NEXTJS_FRONTEND_MIGRATION_NOTE.md"
require_file "docs/rfcs/RFC-005-nextjs-frontend-and-http-api-boundary.md"
require_file "docs/plans/planned/F33-nextjs-frontend-and-http-api-foundation.md"
require_file "docs/specs/F33-nextjs-frontend-and-http-api-foundation/REQUIREMENTS.md"
require_file "docs/specs/F33-nextjs-frontend-and-http-api-foundation/ACCEPTANCE.md"
require_file "src/Fic.Platform.Frontend/package.json"
require_file "src/Fic.Platform.Frontend/app/portal/signup/page.tsx"
require_file "src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"
require_file "tests/Fic.Platform.Web.Tests/MerchantApiTests.cs"

require_text "MapGroup\\(\"/api/v1\"\\)" "src/Fic.Platform.Web/Program.cs"
require_text "/session/complete-signup" "src/Fic.Platform.Web/Program.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
      --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false \
      --filter "FullyQualifiedName~MerchantApiTests"

echo "F33 Next.js and HTTP API foundation validation passed."
