#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F45_PORTAL_IA_AND_CARD_MANAGEMENT_LANE_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-017-portal-ia-and-card-management-lane.md"
  "$ROOT_DIR/docs/plans/active/F45-portal-ia-and-card-management-lane.md"
  "$ROOT_DIR/docs/specs/F45-portal-ia-and-card-management-lane/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F45-portal-ia-and-card-management-lane/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/public-portal-header.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

if ! rg -q "Log in|Sign up|Open site navigation" "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/public-portal-header.tsx"; then
  echo "Expected public header auth actions and burger navigation."
  exit 1
fi

if ! rg -q "Drawer\\.Root" "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"; then
  echo "Expected portal shell burger drawer for utility links."
  exit 1
fi

if ! rg -q "min-w-\\[62rem\\]|Filter by card code|Copy code|Customer cards" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Expected customers table-first management lane in merchant workspace."
  exit 1
fi

echo "F45 portal IA and card management lane validator passed."
