#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F40_ONBOARDING_FLOW_CONVERGENCE_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-012-onboarding-flow-convergence.md"
  "$ROOT_DIR/docs/plans/active/F40-onboarding-flow-convergence.md"
  "$ROOT_DIR/docs/specs/F40-onboarding-flow-convergence/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F40-onboarding-flow-convergence/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/signup/billing/[merchantId]/page.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/scripts/qa-signup-flow.mjs"
  "$ROOT_DIR/src/Fic.Platform.Frontend/scripts/qa-workspace-slices.mjs"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

if ! rg -q "Setup tasks" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Expected Setup tasks card marker in merchant workspace onboarding."
  exit 1
fi

if rg -q "Step 5: shop setup|Step 6: programme template" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Legacy Step 5/Step 6 onboarding headings are still present."
  exit 1
fi

if ! rg -q "setup=shop" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/signup/billing/[merchantId]/page.tsx"; then
  echo "Expected billing handoff to include setup intent query."
  exit 1
fi

echo "F40 onboarding flow convergence validator passed."
