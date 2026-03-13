#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMMITS="${1:-120}"
LIMIT="${2:-15}"

if ! [[ "${COMMITS}" =~ ^[0-9]+$ ]] || ! [[ "${LIMIT}" =~ ^[0-9]+$ ]]; then
  echo "usage: scripts/report-churn-hotspots.sh [commit_window] [result_limit]" >&2
  exit 1
fi

git -C "${ROOT_DIR}" log --name-only --pretty=format: -n "${COMMITS}" \
  | rg -v '^$' \
  | sort \
  | uniq -c \
  | sort -nr \
  | head -n "${LIMIT}"
