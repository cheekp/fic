#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F47_CARD_OPERATIONS_DETAIL_LANE_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-019-card-operations-detail-lane.md"
  "$ROOT_DIR/docs/plans/active/F47-card-operations-detail-lane.md"
  "$ROOT_DIR/docs/specs/F47-card-operations-detail-lane/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F47-card-operations-detail-lane/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

if ! rg -q "cardStatusFilter|Filter status|Reward ready" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Expected card status filter controls in customers lane."
  exit 1
fi

if ! rg -q "handleOpenCardDetail|Card detail:|Recent activity" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Expected card detail drill-in dialog in customers lane."
  exit 1
fi

if ! rg -q "Card JSON|Copy code|Redeem" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Expected row/detail card actions to remain available."
  exit 1
fi

echo "F47 card operations detail lane validator passed."
