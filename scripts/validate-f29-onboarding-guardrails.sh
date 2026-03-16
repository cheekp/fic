#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_CLI_HOME="${ROOT_DIR}/.dotnet_cli"
export NUGET_PACKAGES="${ROOT_DIR}/.nuget/packages"
export MSBuildEnableWorkloadResolver=false

TEST_NUGET_PACKAGES="${NUGET_PACKAGES}"
if [[ ! -d "${TEST_NUGET_PACKAGES}/microsoft.net.test.sdk" && -d "${HOME}/.nuget/packages/microsoft.net.test.sdk" ]]; then
  TEST_NUGET_PACKAGES="${HOME}/.nuget/packages"
fi

require_file() {
  local path="$1"
  if [[ ! -f "${ROOT_DIR}/${path}" ]]; then
    echo "missing file: ${path}" >&2
    exit 1
  fi
}

require_absent() {
  local path="$1"
  if [[ -e "${ROOT_DIR}/${path}" ]]; then
    echo "unexpected file present: ${path}" >&2
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

require_file "docs/plans/active/F29-onboarding-guardrails-and-tiered-billing.md"
require_file "docs/plans/completed/F28-first-time-workspace-focus.md"
require_absent "docs/plans/active/F28-first-time-workspace-focus.md"
require_file "docs/specs/F29-onboarding-guardrails-and-tiered-billing/REQUIREMENTS.md"
require_file "docs/specs/F29-onboarding-guardrails-and-tiered-billing/ACCEPTANCE.md"
require_file "docs/ENGINEERING_HARNESS.md"
require_file "README.md"
require_file "src/Fic.Platform.Web/Components/Layout/OnboardingJourney.razor"
require_file "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SignupPlan.razor"
require_file "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_file "src/Fic.Platform.Web/Program.cs"
require_file "src/Fic.Platform.Web/wwwroot/app.css"
require_file "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"
require_file "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_file "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_file "tests/Fic.Platform.Web.Tests/MerchantAuthBoundaryTests.cs"

require_text "F29-onboarding-guardrails-and-tiered-billing.md" "docs/plans/active/README.md"
require_text "F29 Onboarding Guardrails And Tiered Billing" "docs/plans/active/F29-onboarding-guardrails-and-tiered-billing.md"
require_text "seeded-data shortcut" "docs/specs/F29-onboarding-guardrails-and-tiered-billing/REQUIREMENTS.md"
require_text 'Only the `£19.99/mo` self-serve tier advances into workspace setup.' "docs/specs/F29-onboarding-guardrails-and-tiered-billing/ACCEPTANCE.md"
require_text 'validator: `scripts/validate-f29-onboarding-guardrails.sh`' "docs/ENGINEERING_HARNESS.md"
require_text "onboarding guardrails" "README.md"
require_text "Use demo details" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Step 1 of 5" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Continue to Plan" "src/Fic.Platform.Web/Components/Pages/PortalSignup.razor"
require_text "Step 2 of 5" "src/Fic.Platform.Web/Components/Pages/SignupPlan.razor"
require_text "Continue with Starter" "src/Fic.Platform.Web/Components/Pages/SignupPlan.razor"
require_text "OnboardingJourney" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "paymentMethod" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "Step 3 of 5" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "billing-plan-summary" "src/Fic.Platform.Web/Components/Pages/SignupBilling.razor"
require_text "selectedPlan" "src/Fic.Platform.Web/Program.cs"
require_text "SignupDemoSeedEnabled" "src/Fic.Platform.Web/appsettings.json"
require_text "Complete shop details first" "src/Fic.Platform.Web/Components/Pages/VendorWorkspace.razor"
require_text "tier-card" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "tier-card__flag" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "onboarding-journey__progress" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "signup-panel--plan" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "shop-settings-callout" "src/Fic.Platform.Web/wwwroot/app.css"
require_text "SignupPage_ShowsDemoSeedAction_WhenFeatureFlagIsEnabled" "tests/Fic.Platform.Web.Tests/CompanyBrandSurfaceTests.cs"
require_text "Workspace_ShowsOnboardingJourney_InFirstTimeMode" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "FirstTimeRouteGuard_RequiresShopDetailsBeforeTemplateCreation" "tests/Fic.Platform.Web.Tests/VendorWorkspaceComponentTests.cs"
require_text "ConfigureMerchantAccess_ReturnsAlreadyConfigured_WhenCredentialsExist" "tests/Fic.Platform.Web.Tests/DemoPlatformStateTests.cs"
require_text "CompleteSignupEndpoint_AllowsStarterPlanWithoutBillingCheckbox" "tests/Fic.Platform.Web.Tests/MerchantAuthBoundaryTests.cs"

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet build "${ROOT_DIR}/Fic.sln" --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false

env DOTNET_CLI_HOME="${DOTNET_CLI_HOME}" \
    NUGET_PACKAGES="${TEST_NUGET_PACKAGES}" \
    MSBuildEnableWorkloadResolver="${MSBuildEnableWorkloadResolver}" \
    dotnet test "${ROOT_DIR}/tests/Fic.Platform.Web.Tests/Fic.Platform.Web.Tests.csproj" \
        --disable-build-servers -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RestoreIgnoreFailedSources=true -p:NuGetAudit=false \
        --filter "FullyQualifiedName~CompanyBrandSurfaceTests|FullyQualifiedName~VendorWorkspaceComponentTests|FullyQualifiedName~DemoPlatformStateTests|FullyQualifiedName~MerchantAuthBoundaryTests"

echo "F29 onboarding guardrails validation passed."
