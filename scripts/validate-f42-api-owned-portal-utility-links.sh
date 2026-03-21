#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F42_API_OWNED_PORTAL_UTILITY_LINKS_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-014-api-owned-portal-utility-links.md"
  "$ROOT_DIR/docs/plans/active/F42-api-owned-portal-utility-links.md"
  "$ROOT_DIR/docs/specs/F42-api-owned-portal-utility-links/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F42-api-owned-portal-utility-links/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Contracts/PortalNavigationContracts.cs"
  "$ROOT_DIR/src/Fic.Platform.Web/Services/PortalNavigationContractBuilder.cs"
  "$ROOT_DIR/src/Fic.Platform.Frontend/types/portal-contracts.ts"
  "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"
  "$ROOT_DIR/tests/Fic.Platform.Web.Tests/MerchantApiTests.cs"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

if ! rg -q "PortalUtilityLinkContract" "$ROOT_DIR/src/Fic.Contracts/PortalNavigationContracts.cs"; then
  echo "Expected PortalUtilityLinkContract in shared contracts."
  exit 1
fi

if ! rg -q "DefaultUtilityLinks" "$ROOT_DIR/src/Fic.Platform.Web/Services/PortalNavigationContractBuilder.cs"; then
  echo "Expected API-owned utility links in portal navigation builder."
  exit 1
fi

if ! rg -q "utilityLinks\\?: PortalUtilityLinkContract\\[]" "$ROOT_DIR/src/Fic.Platform.Frontend/types/portal-contracts.ts"; then
  echo "Expected utilityLinks in frontend portal navigation type."
  exit 1
fi

if ! rg -q "utilityLinks=\\{portalNav\\?\\.utilityLinks\\}" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal" -g "*.tsx"; then
  echo "Expected portal pages to pass utilityLinks contract to PortalShell."
  exit 1
fi

if ! rg -q "fallbackUtilityLinks" "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"; then
  echo "Expected fallback utility links in PortalShell."
  exit 1
fi

if ! rg -q "contract.UtilityLinks" "$ROOT_DIR/tests/Fic.Platform.Web.Tests/MerchantApiTests.cs"; then
  echo "Expected API tests to assert utility links."
  exit 1
fi

echo "F42 API-owned portal utility links validator passed."
