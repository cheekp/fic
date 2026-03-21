#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F39_PORTAL_UTILITY_IA_AND_WORKFLOW_POLISH_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-011-portal-utility-ia-and-workflow-polish.md"
  "$ROOT_DIR/docs/plans/active/F39-portal-utility-ia-and-workflow-polish.md"
  "$ROOT_DIR/docs/specs/F39-portal-utility-ia-and-workflow-polish/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F39-portal-utility-ia-and-workflow-polish/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/blogs/page.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/training/page.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/consultancy/page.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/account/page.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/billing/page.tsx"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

echo "F39 portal utility IA and workflow polish validator passed."
