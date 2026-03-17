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

require_file "docs/architecture/F32_APP_CSS_PARTIAL_SPLIT_NOTE.md"
require_file "docs/rfcs/RFC-004-app-css-partials-and-bundled-budgeting.md"
require_file "docs/plans/active/F32-app-css-partial-split-and-bundle-guardrails.md"
require_file "docs/specs/F32-app-css-partial-split-and-bundle-guardrails/REQUIREMENTS.md"
require_file "docs/specs/F32-app-css-partial-split-and-bundle-guardrails/ACCEPTANCE.md"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "src/Fic.Platform.Web/wwwroot/styles/app-auxiliary-surfaces.css"
require_file "scripts/render-css-bundle.py"
require_file "scripts/validate-css-budget.sh"
require_file "tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs"

require_text "F32-app-css-partial-split-and-bundle-guardrails.md" "docs/plans/active/README.md"
require_text "F32_APP_CSS_PARTIAL_SPLIT_NOTE.md" "docs/architecture/README.md"
require_text "RFC-004-app-css-partials-and-bundled-budgeting.md" "docs/rfcs/README.md"
require_text 'validator: `scripts/validate-f32-app-css-partial-split.sh`' "docs/ENGINEERING_HARNESS.md"
require_text '@import url\("styles/app-auxiliary-surfaces.css"\)' "src/Fic.Platform.Web/wwwroot/app.css"
require_text "LoadGlobalStylesheetBundle" "tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs"

"${ROOT_DIR}/scripts/validate-ux-surface.sh"

echo "F32 app.css partial split validation passed."
