#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
MAX_LINES=300

usage() {
  cat <<EOF
Usage: $(basename "$0") [--max-lines <N>]

Validate *.cs file size under src/ and tests/.

Options:
  --max-lines <N>  Maximum allowed lines per file (default: 300)
  -h, --help       Show this help
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --max-lines)
      if [[ $# -lt 2 ]]; then
        echo "missing value for --max-lines" >&2
        exit 1
      fi
      MAX_LINES="$2"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "unknown option: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

if ! [[ "$MAX_LINES" =~ ^[0-9]+$ ]] || [[ "$MAX_LINES" -le 0 ]]; then
  echo "--max-lines must be a positive integer" >&2
  exit 1
fi

FAILED=0
FOUND=0
while IFS= read -r -d '' file; do
  FOUND=1
  lines="$(wc -l < "$file" | tr -d ' ')"
  if [[ "$lines" -gt "$MAX_LINES" ]]; then
    rel_path="${file#"${ROOT_DIR}/"}"
    echo "file exceeds line limit (${lines} > ${MAX_LINES}): ${rel_path}" >&2
    FAILED=1
  fi
done < <(find "${ROOT_DIR}/src" "${ROOT_DIR}/tests" -type f -name "*.cs" -print0)

if [[ "$FOUND" -eq 0 ]]; then
  echo "No C# files found under src/ and tests/."
  exit 0
fi

if [[ "$FAILED" -ne 0 ]]; then
  echo "C# file size validation failed." >&2
  exit 1
fi

echo "C# file size validation passed (max ${MAX_LINES} lines per file)."
