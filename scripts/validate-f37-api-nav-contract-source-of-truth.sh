#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F37_API_NAV_CONTRACT_SOURCE_OF_TRUTH_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-009-api-nav-contract-source-of-truth.md"
  "$ROOT_DIR/docs/plans/active/F37-api-nav-contract-source-of-truth.md"
  "$ROOT_DIR/docs/specs/F37-api-nav-contract-source-of-truth/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F37-api-nav-contract-source-of-truth/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Contracts/PortalNavigationContracts.cs"
  "$ROOT_DIR/src/Fic.Platform.Web/Services/PortalNavigationContractBuilder.cs"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

echo "F37 api nav contract source-of-truth validator passed."
