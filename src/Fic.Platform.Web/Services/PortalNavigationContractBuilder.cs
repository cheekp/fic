using System.Text.RegularExpressions;
using Fic.Contracts;

namespace Fic.Platform.Web.Services;

public sealed class PortalNavigationContractBuilder
{
    private static readonly Regex HexColorPattern = new("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", RegexOptions.Compiled);

    private static readonly PortalThemeContract DefaultTheme = new(
        "#1f3731",
        "#f4c15d",
        "rgb(255 250 240 / 0.82)",
        "#14211d",
        "rounded",
        "soft");

    public PortalNavigationContract BuildSignupNavigation(string? step, Guid? merchantId, MerchantWorkspaceSnapshot? workspace)
    {
        var activeKey = step switch
        {
            "plan" => "plan",
            "owner" => "billing",
            "billing" => "billing",
            _ => "signup",
        };

        var resolvedMerchantId = workspace?.Merchant.MerchantId ?? merchantId;
        var hasMerchant = resolvedMerchantId.HasValue;
        var merchantIdValue = resolvedMerchantId?.ToString("D");
        var planHref = hasMerchant ? $"/portal/signup/plan/{merchantIdValue}" : "#";
        var billingHref = hasMerchant ? $"/portal/signup/billing/{merchantIdValue}?plan=starter" : "#";
        var ownerAccessConfigured = workspace?.SetupChecklist.OwnerAccessConfigured ?? false;

        var accountComplete = activeKey is "plan" or "billing" || hasMerchant;
        var planComplete = activeKey is "billing" || ownerAccessConfigured;
        var ownerComplete = ownerAccessConfigured;
        var billingComplete = ownerAccessConfigured;

        var items = (IReadOnlyList<PortalNavItemContract>)
        [
            new PortalNavItemContract(
                "signup",
                "Create account",
                "/portal/signup",
                null,
                accountComplete,
                false),
            new PortalNavItemContract(
                "plan",
                "Choose plan",
                planHref,
                null,
                planComplete,
                !hasMerchant && activeKey == "signup"),
            new PortalNavItemContract(
                "billing",
                "Billing",
                billingHref,
                null,
                billingComplete,
                !hasMerchant && activeKey != "billing"),
        ];

        var roadmap = BuildRoadmap(
            activeRoadmapKey: ToRoadmapKeyFromSignup(step),
            merchantId: resolvedMerchantId,
            accountComplete: accountComplete,
            planComplete: planComplete,
            ownerComplete: ownerComplete,
            billingComplete: billingComplete,
            shopComplete: workspace?.SetupChecklist.ShopDetailsComplete ?? false,
            programmeComplete: workspace?.SetupChecklist.HasAnyProgramme ?? false);

        return new PortalNavigationContract("signup", activeKey, ResolveTheme(workspace), items, roadmap);
    }

    public PortalNavigationContract BuildWorkspaceNavigation(
        string? step,
        Guid merchantId,
        MerchantWorkspaceSnapshot workspace,
        Guid? selectedProgrammeId)
    {
        var activeKey = step switch
        {
            "configure" => "configure",
            "customers" => "customers",
            _ => "operate",
        };

        var programmeId = selectedProgrammeId ?? workspace.SelectedProgramme?.ProgrammeId;
        var hasProgramme = workspace.SetupChecklist.HasAnyProgramme && programmeId.HasValue;
        var cardsCount = workspace.SelectedProgrammeCards.Count;
        var basePath = $"/portal/merchant/{merchantId:D}";
        var href = new Func<string, string>(routeStep =>
            programmeId.HasValue
                ? $"{basePath}?programmeSection={routeStep}&programme={programmeId:D}"
                : $"{basePath}?programmeSection={routeStep}");

        var shopComplete = workspace.SetupChecklist.ShopDetailsComplete;
        var programmeComplete = workspace.SetupChecklist.HasAnyProgramme;
        var items = (IReadOnlyList<PortalNavItemContract>)
        [
            new PortalNavItemContract(
                "operate",
                "Operate",
                href("operate"),
                cardsCount > 0 ? cardsCount.ToString() : null,
                workspace.SetupChecklist.HasAnyProgramme,
                false),
            new PortalNavItemContract(
                "configure",
                "Configure",
                href("configure"),
                null,
                shopComplete && workspace.SetupChecklist.BrandComplete,
                !hasProgramme),
            new PortalNavItemContract(
                "customers",
                "Customers",
                href("customers"),
                null,
                workspace.SetupChecklist.OwnerAccessConfigured && workspace.SetupChecklist.JoinReady,
                !hasProgramme),
        ];

        var roadmap = BuildRoadmap(
            activeRoadmapKey: ResolveWorkspaceRoadmapKey(workspace),
            merchantId: merchantId,
            accountComplete: true,
            planComplete: true,
            ownerComplete: workspace.SetupChecklist.OwnerAccessConfigured,
            billingComplete: workspace.SetupChecklist.OwnerAccessConfigured,
            shopComplete: shopComplete,
            programmeComplete: programmeComplete);

        return new PortalNavigationContract("workspace", activeKey, ResolveTheme(workspace), items, roadmap);
    }

    private static string ToRoadmapKeyFromSignup(string? step) =>
        step switch
        {
            "plan" => "plan",
            "owner" => "owner",
            "billing" => "billing",
            _ => "account",
        };

    private static string ResolveWorkspaceRoadmapKey(MerchantWorkspaceSnapshot workspace)
    {
        if (!workspace.SetupChecklist.OwnerAccessConfigured)
        {
            return "owner";
        }

        if (!workspace.SetupChecklist.ShopDetailsComplete)
        {
            return "shop";
        }

        if (!workspace.SetupChecklist.HasAnyProgramme)
        {
            return "programme";
        }

        return "programme";
    }

    private static PortalRoadmapContract BuildRoadmap(
        string activeRoadmapKey,
        Guid? merchantId,
        bool accountComplete,
        bool planComplete,
        bool ownerComplete,
        bool billingComplete,
        bool shopComplete,
        bool programmeComplete)
    {
        var merchantPath = merchantId.HasValue ? $"/portal/merchant/{merchantId:D}" : null;
        var planPath = merchantId.HasValue ? $"/portal/signup/plan/{merchantId:D}" : "#";
        var ownerPath = merchantId.HasValue ? $"/portal/signup/billing/{merchantId:D}?plan=starter&stage=owner" : "#";
        var billingPath = merchantId.HasValue ? $"/portal/signup/billing/{merchantId:D}?plan=starter&stage=billing" : "#";
        var shopPath = merchantPath is null ? "#" : $"{merchantPath}?programmeSection=configure";
        var programmePath = merchantPath is null ? "#" : $"{merchantPath}?programmeSection=operate";

        var steps = (IReadOnlyList<PortalRoadmapStepContract>)
        [
            new PortalRoadmapStepContract("account", "Create account", null, "/portal/signup", accountComplete, activeRoadmapKey == "account", true),
            new PortalRoadmapStepContract("plan", "Choose plan", null, planPath, planComplete, activeRoadmapKey == "plan", merchantId.HasValue),
            new PortalRoadmapStepContract("owner", "Owner access", "Owner", ownerPath, ownerComplete, activeRoadmapKey == "owner", merchantId.HasValue),
            new PortalRoadmapStepContract("billing", "Billing", "Billing", billingPath, billingComplete, activeRoadmapKey == "billing", merchantId.HasValue),
            new PortalRoadmapStepContract("shop", "Shop details", null, shopPath, shopComplete, activeRoadmapKey == "shop", merchantId.HasValue),
            new PortalRoadmapStepContract("programme", "Programme template", null, programmePath, programmeComplete, activeRoadmapKey == "programme", merchantId.HasValue),
        ];

        return new PortalRoadmapContract(
            activeRoadmapKey,
            steps.Count(step => step.IsComplete),
            steps.Count,
            null,
            steps);
    }

    private static PortalThemeContract ResolveTheme(MerchantWorkspaceSnapshot? workspace)
    {
        if (workspace is null)
        {
            return DefaultTheme;
        }

        var primary = IsHexColor(workspace.BrandProfile.PrimaryColor) ? workspace.BrandProfile.PrimaryColor : DefaultTheme.Primary;
        var accent = IsHexColor(workspace.BrandProfile.AccentColor) ? workspace.BrandProfile.AccentColor : DefaultTheme.Accent;

        return DefaultTheme with
        {
            Primary = primary,
            Accent = accent,
        };
    }

    private static bool IsHexColor(string? value) =>
        !string.IsNullOrWhiteSpace(value) && HexColorPattern.IsMatch(value.Trim());
}
