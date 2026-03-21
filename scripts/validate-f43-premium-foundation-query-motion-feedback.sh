#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "$ROOT_DIR/docs/architecture/F43_PREMIUM_FOUNDATION_QUERY_MOTION_FEEDBACK_NOTE.md"
  "$ROOT_DIR/docs/rfcs/RFC-015-premium-foundation-query-motion-feedback.md"
  "$ROOT_DIR/docs/plans/active/F43-premium-foundation-query-motion-feedback.md"
  "$ROOT_DIR/docs/specs/F43-premium-foundation-query-motion-feedback/REQUIREMENTS.md"
  "$ROOT_DIR/docs/specs/F43-premium-foundation-query-motion-feedback/ACCEPTANCE.md"
  "$ROOT_DIR/src/Fic.Platform.Frontend/components/providers/app-providers.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/components/ui/sonner.tsx"
  "$ROOT_DIR/src/Fic.Platform.Frontend/lib/queries.ts"
  "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"
)

for file in "${required_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file"
    exit 1
  fi
done

if ! rg -q "@tanstack/react-query" "$ROOT_DIR/src/Fic.Platform.Frontend/package.json"; then
  echo "Expected @tanstack/react-query dependency."
  exit 1
fi

if ! rg -q "\"sonner\"" "$ROOT_DIR/src/Fic.Platform.Frontend/package.json"; then
  echo "Expected sonner dependency."
  exit 1
fi

if ! rg -q "\"vaul\"" "$ROOT_DIR/src/Fic.Platform.Frontend/package.json"; then
  echo "Expected vaul dependency."
  exit 1
fi

if ! rg -q "QueryClientProvider" "$ROOT_DIR/src/Fic.Platform.Frontend/components/providers/app-providers.tsx"; then
  echo "Expected QueryClientProvider in app providers."
  exit 1
fi

if ! rg -q "useSignupPortalNavigationQuery|useWorkspacePortalNavigationQuery|useWorkspaceSnapshotQuery" "$ROOT_DIR/src/Fic.Platform.Frontend/app/portal" -g "*.tsx"; then
  echo "Expected portal pages to use query hooks."
  exit 1
fi

if ! rg -q "Drawer.Root" "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"; then
  echo "Expected Vaul drawer usage in portal shell."
  exit 1
fi

if ! rg -q "motion\\.div" "$ROOT_DIR/src/Fic.Platform.Frontend/components/layout/portal-shell.tsx"; then
  echo "Expected motion transition in portal shell."
  exit 1
fi

echo "F43 premium foundation query/motion/feedback validator passed."
