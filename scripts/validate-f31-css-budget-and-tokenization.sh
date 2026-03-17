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

require_file "docs/architecture/F31_CSS_BUDGET_AND_TOKEN_DISCIPLINE_NOTE.md"
require_file "docs/rfcs/RFC-003-css-budget-and-tokenization-discipline.md"
require_file "docs/plans/active/F31-css-budget-and-tokenization-discipline.md"
require_file "docs/specs/F31-css-budget-and-tokenization-discipline/REQUIREMENTS.md"
require_file "docs/specs/F31-css-budget-and-tokenization-discipline/ACCEPTANCE.md"
require_file "docs/runbooks/UX_QA_PLAYBOOK.md"
require_file "scripts/validate-css-budget.sh"
require_file "scripts/validate-ux-surface.sh"
require_file "tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs"

require_text "F31-css-budget-and-tokenization-discipline.md" "docs/plans/active/README.md"
require_text "F31_CSS_BUDGET_AND_TOKEN_DISCIPLINE_NOTE.md" "docs/architecture/README.md"
require_text "RFC-003-css-budget-and-tokenization-discipline.md" "docs/rfcs/README.md"
require_text 'validator: `scripts/validate-f31-css-budget-and-tokenization.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "scripts/validate-css-budget.sh" "docs/runbooks/UX_QA_PLAYBOOK.md"
require_text "AppCss_StaysWithinBudgetAndTokenUsageFloors" "tests/Fic.Platform.Web.Tests/UxQualityGateTests.cs"

"${ROOT_DIR}/scripts/validate-ux-surface.sh"

echo "F31 CSS budgeting and tokenization validation passed."
