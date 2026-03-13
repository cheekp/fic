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

require_exec() {
  local path="$1"
  if [[ ! -x "${ROOT_DIR}/${path}" ]]; then
    echo "script is not executable: ${path}" >&2
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

require_file "docs/plans/active/F09-delivery-guardrails.md"
require_file "docs/specs/F09-delivery-guardrails/REQUIREMENTS.md"
require_file "docs/specs/F09-delivery-guardrails/ACCEPTANCE.md"
require_file "docs/specs/SEAM_CHECKLIST_TEMPLATE.md"

require_file "scripts/dev-preflight.sh"
require_file "scripts/report-churn-hotspots.sh"
require_exec "scripts/dev-preflight.sh"
require_exec "scripts/report-churn-hotspots.sh"

require_text "preflight" "docs/ENGINEERING_HARNESS.md"
require_text "hotspot" "docs/ENGINEERING_HARNESS.md"

"${ROOT_DIR}/scripts/dev-preflight.sh"
"${ROOT_DIR}/scripts/report-churn-hotspots.sh" 60 10 >/dev/null

echo "F09 delivery guardrails validation passed."
