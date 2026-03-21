#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F38_ROADMAP_UNIFIED_WITH_NAV_CONTRACT_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-010-roadmap-unified-with-nav-contract.md"
  "$ROOT_DIR/docs/plans/active/F38-roadmap-unified-with-nav-contract.md"
  "$ROOT_DIR/docs/specs/F38-roadmap-unified-with-nav-contract/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F38-roadmap-unified-with-nav-contract/ACCEPTANCE.md"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

echo "F38 roadmap unified-with-nav-contract validator passed."
