#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F44_OWNER_BILLING_SETUP_LANE_CLEANUP_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-016-owner-billing-setup-lane-cleanup.md"
  "$ROOT_DIR/docs/plans/active/F44-owner-billing-and-setup-lane-cleanup.md"
  "$ROOT_DIR/docs/specs/F44-owner-billing-and-setup-lane-cleanup/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F44-owner-billing-and-setup-lane-cleanup/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/signup/billing/[merchantId]/page.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

if ! rg -q "Step 3|Owner access" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/signup/billing/[merchantId]/page.tsx"; then
  echo "Expected owner/billing compact step indicator in signup billing page."
  exit 1
fi

if ! rg -q "setup-lane|Step 5: complete shop setup|Step 6: choose programme template" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Expected single setup lane card in workspace onboarding flow."
  exit 1
fi

if ! rg -q "Open setup blade" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Expected setup blade fast path copy in workspace onboarding card."
  exit 1
fi

echo "F44 owner/billing and setup lane cleanup validator passed."
