#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F46_ONBOARDING_ENTRY_FOCUS_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-018-onboarding-entry-focus.md"
  "$ROOT_DIR/docs/plans/active/F46-onboarding-entry-focus.md"
  "$ROOT_DIR/docs/specs/F46-onboarding-entry-focus/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F46-onboarding-entry-focus/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/signup/page.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/signup/plan/[merchantId]/page.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/signup/billing/[merchantId]/page.tsx"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

if ! rg -q 'headerMode\\?: "workspace" \\| "onboarding"' "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"; then
  echo "Expected onboarding header mode contract in PortalShell props."
  exit 1
fi

if ! rg -q 'headerMode="onboarding"' "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal/signup" -g "*.tsx"; then
  echo "Expected signup surfaces to opt into onboarding header mode."
  exit 1
fi

if ! rg -q 'Open portal links' "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"; then
  echo "Expected workspace portal link drawer to remain available."
  exit 1
fi

echo "F46 onboarding entry focus validator passed."
