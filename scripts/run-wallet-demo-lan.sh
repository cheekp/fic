#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_CLI_HOME="${ROOT_DIR}/.dotnet_cli"
export NUGET_PACKAGES="${ROOT_DIR}/.nuget/packages"
export MSBuildEnableWorkloadResolver=false

get_lan_ip() {
  local value=""
  value="$(ipconfig getifaddr en0 2>/dev/null || true)"
  if [[ -z "${value}" ]]; then
    value="$(ipconfig getifaddr en1 2>/dev/null || true)"
  fi
  printf '%s' "${value}"
}

LAN_IP="$(get_lan_ip)"

echo "Starting FIC wallet demo host on the local network."
if [[ -n "${LAN_IP}" ]]; then
  echo "Open on your Mac: http://${LAN_IP}:5276/portal/signup"
else
  echo "Could not determine the LAN IP automatically. Open the app using your Mac's current LAN address."
fi
echo "If Apple Wallet signing is configured in user secrets, the join flow will download a signed .pkpass."
echo "If signing is incomplete, the existing preview fallback remains active."

cd "${ROOT_DIR}"
dotnet run --project src/Fic.Platform.Web/Fic.Platform.Web.csproj --launch-profile lan-http
