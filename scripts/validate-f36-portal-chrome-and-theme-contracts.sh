#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "docs/architecture/F36_PORTAL_CHROME_AND_THEME_CONTRACTS_NOTE.md"
  "docs/rfcs/RFC-008-portal-chrome-and-theme-contracts.md"
  "docs/plans/active/F36-portal-chrome-and-theme-contracts.md"
  "docs/specs/F36-portal-chrome-and-theme-contracts/REQUIREMENTS.md"
  "docs/specs/F36-portal-chrome-and-theme-contracts/ACCEPTANCE.md"
  "src/Fic.Platform.Frontend/types/portal-contracts.ts"
  "src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"
  "src/Fic.Platform.Frontend/components/layout/portal-surface.tsx"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "${ROOT_DIR}/${file}" ]]; then
    echo "Missing required file: ${file}" >&2
    exit 1
  fi
done

echo "F36 portal chrome and theme contracts validator passed."
