#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F41_WORKSPACE_NEXT_ACTION_CONTRACT_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-013-workspace-next-action-contract.md"
  "$ROOT_DIR/docs/plans/active/F41-workspace-next-action-contract.md"
  "$ROOT_DIR/docs/specs/F41-workspace-next-action-contract/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F41-workspace-next-action-contract/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Contracts/PortalNavigationContracts.cs"
  "$ROOT_DIR/src/Fic.Platform.Web/Services/PortalNavigationContractBuilder.cs"
  "$ROOT_DIR/src/Fic.Platform.Frontend/types/portal-contracts.ts"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"
  "$ROOT_DIR/tests/Fic.Platform.Web.Tests/MerchantApiTests.cs"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

if ! rg -q "PortalNextActionContract" "$ROOT_DIR/src/Fic.Contracts/PortalNavigationContracts.cs"; then
  echo "Expected PortalNextActionContract in shared contracts."
  exit 1
fi

if ! rg -q "BuildWorkspaceNextAction" "$ROOT_DIR/src/Fic.Platform.Web/Services/PortalNavigationContractBuilder.cs"; then
  echo "Expected BuildWorkspaceNextAction in portal navigation builder."
  exit 1
fi

if ! rg -q "nextAction" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/merchant/[merchantId]/page.tsx"; then
  echo "Expected workspace page to consume nextAction contract."
  exit 1
fi

echo "F41 workspace next-action contract validator passed."
