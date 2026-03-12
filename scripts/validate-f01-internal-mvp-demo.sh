#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

require_file() {
  local path="$1"
  if [[ ! -f "${ROOT_DIR}/${path}" ]]; then
    echo "missing file: ${path}" >&2
    exit 1
  fi
}

require_text() {
  local pattern="$1"
  local path="$2"
  if ! rg -q "${pattern}" "${ROOT_DIR}/${path}"; then
    echo "missing pattern '${pattern}' in ${path}" >&2
    exit 1
  fi
}

require_file "docs/ENGINEERING_HARNESS.md"
require_file "docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md"
require_file "docs/rfcs/RFC-001-platform-architecture.md"
require_file "docs/specs/F01-internal-mvp-demo/REQUIREMENTS.md"
require_file "docs/specs/F01-internal-mvp-demo/ACCEPTANCE.md"
require_file "docs/specs/F01-internal-mvp-demo/contracts/INTERNAL_MVP_DEMO_CONTRACT.md"

require_text "SignalR" "docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md"
require_text "Aspire" "docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md"
require_text "2/5 coffees" "docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md"
require_text "Apple Wallet" "docs/specs/F01-internal-mvp-demo/REQUIREMENTS.md"
require_text "2/5 coffees" "docs/specs/F01-internal-mvp-demo/REQUIREMENTS.md"
require_text "vendor PWA and wallet pass" "docs/specs/F01-internal-mvp-demo/ACCEPTANCE.md"

echo "F01 internal MVP/demo validation passed."
