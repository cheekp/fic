#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CSS_PATH="${ROOT_DIR}/src/Fic.Platform.Web/wwwroot/app.css"

if [[ ! -f "${CSS_PATH}" ]]; then
  echo "missing file: src/Fic.Platform.Web/wwwroot/app.css" >&2
  exit 1
fi

MAX_LINES="${FIC_CSS_MAX_LINES:-3700}"
MAX_BYTES="${FIC_CSS_MAX_BYTES:-90000}"
MAX_LITERAL_OUTSIDE_ROOT="${FIC_CSS_MAX_LITERAL_OUTSIDE_ROOT:-145}"
MIN_TOKEN_DEFS="${FIC_CSS_MIN_TOKEN_DEFS:-45}"
MIN_TOKEN_REFS="${FIC_CSS_MIN_TOKEN_REFS:-400}"

line_count="$(wc -l < "${CSS_PATH}" | tr -d ' ')"
byte_count="$(wc -c < "${CSS_PATH}" | tr -d ' ')"

# Count token declarations in the root block.
token_defs="$(rg -No '^\s*--[a-z0-9\-]+\s*:' "${CSS_PATH}" | wc -l | tr -d ' ')"
# Count token references across stylesheet rules.
token_refs="$(rg -No 'var\(--[a-z0-9\-]+\)' "${CSS_PATH}" | wc -l | tr -d ' ')"

# Count raw color literals outside of the :root token declaration block.
outside_root_literal_count="$({
  awk '
    BEGIN { in_root = 0 }
    /^:root[[:space:]]*\{/ { in_root = 1; next }
    in_root && /^}/ { in_root = 0; next }
    !in_root { print }
  ' "${CSS_PATH}" | rg -No '#[0-9a-fA-F]{3,8}\b|rgba?\([^)]*\)|hsla?\([^)]*\)' | wc -l
} | tr -d ' ')"

echo "CSS budget metrics:"
echo "  lines=${line_count}/${MAX_LINES}"
echo "  bytes=${byte_count}/${MAX_BYTES}"
echo "  raw_literals_outside_root=${outside_root_literal_count}/${MAX_LITERAL_OUTSIDE_ROOT}"
echo "  token_defs=${token_defs} (min ${MIN_TOKEN_DEFS})"
echo "  token_refs=${token_refs} (min ${MIN_TOKEN_REFS})"

if (( line_count > MAX_LINES )); then
  echo "CSS budget exceeded: app.css line count (${line_count}) is above ${MAX_LINES}." >&2
  exit 1
fi

if (( byte_count > MAX_BYTES )); then
  echo "CSS budget exceeded: app.css byte size (${byte_count}) is above ${MAX_BYTES}." >&2
  exit 1
fi

if (( outside_root_literal_count > MAX_LITERAL_OUTSIDE_ROOT )); then
  echo "Tokenization budget exceeded: raw color literals outside :root (${outside_root_literal_count}) are above ${MAX_LITERAL_OUTSIDE_ROOT}." >&2
  echo "Move repeated colors into :root tokens and reference them with var(--...)." >&2
  exit 1
fi

if (( token_defs < MIN_TOKEN_DEFS )); then
  echo "Token floor not met: found ${token_defs} token declarations, need at least ${MIN_TOKEN_DEFS}." >&2
  exit 1
fi

if (( token_refs < MIN_TOKEN_REFS )); then
  echo "Token floor not met: found ${token_refs} token references, need at least ${MIN_TOKEN_REFS}." >&2
  exit 1
fi

echo "CSS budget validation passed."
