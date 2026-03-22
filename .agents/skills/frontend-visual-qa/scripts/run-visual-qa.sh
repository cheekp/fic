#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
FRONTEND_DIR="${ROOT_DIR}/src/Fic.Platform.Frontend"
MODE="${1:-full}"

run_build() {
  (cd "${FRONTEND_DIR}" && npm run build)
}

run_signup() {
  (cd "${FRONTEND_DIR}" && npm run qa:signup-flow)
}

run_workspace() {
  (cd "${FRONTEND_DIR}" && npm run qa:workspace-slices)
}

run_baseline() {
  (cd "${FRONTEND_DIR}" && npm run qa:visual-baseline:compare)
}

case "${MODE}" in
  full)
    run_build
    run_signup
    run_workspace
    ;;
  build)
    run_build
    ;;
  signup)
    run_signup
    ;;
  workspace)
    run_workspace
    ;;
  baseline)
    run_baseline
    ;;
  *)
    echo "Unknown mode: ${MODE}" >&2
    echo "Use one of: full, build, signup, workspace, baseline" >&2
    exit 1
    ;;
esac
