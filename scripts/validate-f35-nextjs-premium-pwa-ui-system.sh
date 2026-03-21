#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
FRONTEND_DIR="${ROOT_DIR}/src/Fic.Platform.Frontend"
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

require_file "docs/architecture/F35_NEXTJS_PREMIUM_PWA_UI_NOTE.md"
require_file "docs/rfcs/RFC-007-nextjs-premium-pwa-ui-system.md"
require_file "docs/plans/planned/F35-nextjs-premium-pwa-ui-system.md"
require_file "docs/specs/F35-nextjs-premium-pwa-ui-system/REQUIREMENTS.md"
require_file "docs/specs/F35-nextjs-premium-pwa-ui-system/ACCEPTANCE.md"

require_file "src/Fic.Platform.Frontend/tailwind.config.ts"
require_file "src/Fic.Platform.Frontend/postcss.config.mjs"
require_file "src/Fic.Platform.Frontend/app/manifest.ts"
require_file "src/Fic.Platform.Frontend/public/sw.js"
require_file "src/Fic.Platform.Frontend/app/offline/page.tsx"

require_file "src/Fic.Platform.Frontend/components/ui/button.tsx"
require_file "src/Fic.Platform.Frontend/components/ui/card.tsx"
require_file "src/Fic.Platform.Frontend/components/ui/input.tsx"
require_file "src/Fic.Platform.Frontend/components/ui/label.tsx"
require_file "src/Fic.Platform.Frontend/components/ui/tabs.tsx"
require_file "src/Fic.Platform.Frontend/components/ui/select.tsx"
require_file "src/Fic.Platform.Frontend/components/ui/badge.tsx"

require_text "tailwindcss" "src/Fic.Platform.Frontend/package.json"
require_text "radix" "src/Fic.Platform.Frontend/package.json"
require_text "Tabs" "src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"

(
  cd "${FRONTEND_DIR}"
  npm run build
)

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
      --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false \
      --filter "FullyQualifiedName~MerchantApiTests|FullyQualifiedName~MerchantAuthBoundaryTests"

echo "F35 Next.js premium PWA UI validation passed."
