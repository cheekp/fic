#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F48_CARD_LIFECYCLE_BULK_OPERATIONS_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-020-card-lifecycle-and-bulk-operations.md"
  "$ROOT_DIR/docs/plans/active/F48-card-lifecycle-bulk-operations.md"
  "$ROOT_DIR/docs/specs/F48-card-lifecycle-bulk-operations/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F48-card-lifecycle-bulk-operations/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Platform.Web/Program.cs"
  "$ROOT_DIR/src/Fic.Platform.Web/Services/DemoPlatformState.cs"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

if ! rg -q "cards/.*/lifecycle|cards/lifecycle" "$ROOT_DIR/src/Fic.Platform.Web/Program.cs"; then
  echo "Expected lifecycle API endpoints in Program.cs"
  exit 1
fi

if ! rg -q "ApplyCardLifecycleAction|ApplyBulkCardLifecycleAction|CustomerCardLifecycleState" "$ROOT_DIR/src/Fic.Platform.Web/Services/DemoPlatformState.cs"; then
  echo "Expected lifecycle state handling in DemoPlatformState."
  exit 1
fi

if ! rg -q "selectedCardIds|Suspend selected|Reactivate selected|Archive selected" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Expected bulk lifecycle UI controls in customers lane."
  exit 1
fi

echo "F48 card lifecycle and bulk operations validator passed."
