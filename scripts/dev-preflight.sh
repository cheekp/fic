#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

require_file() {
  local path="$1"
  if [[ ! -f "${ROOT_DIR}/${path}" ]]; then
    echo "missing required file: ${path}" >&2
    exit 1
  fi
}

require_dir() {
  local path="$1"
  if [[ ! -d "${ROOT_DIR}/${path}" ]]; then
    echo "missing required directory: ${path}" >&2
    exit 1
  fi
}

check_edit_form_names() {
  local missing=0
  local file
  while IFS= read -r file; do
    if ! rg -q "FormName=\"" "${file}"; then
      echo "EditForm missing FormName: ${file#"${ROOT_DIR}/"}" >&2
      missing=1
    fi
  done < <(rg -l "<EditForm" "${ROOT_DIR}/src/Fic.Platform.Web/Components" --glob '*.razor')

  if [[ ${missing} -ne 0 ]]; then
    exit 1
  fi
}

require_file "src/Fic.AppHost/Properties/launchSettings.json"
require_file "src/Fic.Platform.Web/Properties/launchSettings.json"
require_file "src/Fic.Platform.Web/Program.cs"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_dir "src/Fic.Platform.Web/wwwroot"

check_edit_form_names

echo "dev preflight passed."
