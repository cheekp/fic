#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_CLI_HOME="${ROOT_DIR}/.dotnet_cli"
export NUGET_PACKAGES="${ROOT_DIR}/.nuget/packages"
export MSBuildEnableWorkloadResolver=false

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
require_file "Fic.sln"
require_file "src/Fic.AppHost/Fic.AppHost.csproj"
require_file "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_file "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_file "src/Fic.Platform.Web/Components/Pages/Join.razor"
require_file "src/Fic.Platform.Web/Components/Pages/WalletCard.razor"

require_text "SignalR" "docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md"
require_text "Aspire" "docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md"
require_text "2/5 coffees" "docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md"
require_text "Merchant Accounts" "docs/architecture/FIC_PLATFORM_ARCHITECTURE_DRAFT.md"
require_text "Apple Wallet" "docs/specs/F01-internal-mvp-demo/REQUIREMENTS.md"
require_text "2/5 coffees" "docs/specs/F01-internal-mvp-demo/REQUIREMENTS.md"
require_text "vendor-scans-pass" "docs/specs/F01-internal-mvp-demo/ACCEPTANCE.md"
require_text "MerchantAccountCreated" "src/Fic.MerchantAccounts/MerchantAccount.cs"
require_text "VisitAwarded" "src/Fic.LoyaltyCore/LoyaltyProgramme.cs"
require_text "@page \"/join/\\{JoinCode\\}\"" "src/Fic.Platform.Web/Components/Pages/Join.razor"
require_text "@page \"/wallet/card/\\{CardId:guid\\}\"" "src/Fic.Platform.Web/Components/Pages/WalletCard.razor"
require_text "Award visit" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"

if [[ ! -f "${ROOT_DIR}/src/Fic.AppHost/obj/project.assets.json" ]]; then
  env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
      NUGET_PACKAGES="${NUGET_PACKAGES}" \
      MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
      dotnet restore "${ROOT_DIR}/src/Fic.AppHost/Fic.AppHost.csproj"
fi

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build --no-restore --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false "${ROOT_DIR}/Fic.sln"

echo "F01 internal MVP/demo validation passed."
