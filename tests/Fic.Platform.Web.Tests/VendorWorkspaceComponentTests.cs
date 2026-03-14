using Bunit;
using Fic.Contracts;
using Fic.Platform.Web.Components.Pages;
using Fic.Platform.Web.Services;
using Fic.WalletPasses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Fic.Platform.Web.Tests;

public sealed class VendorWorkspaceComponentTests
{
    private const string BaseUri = "https://demo.fic.test/";
    private const string FallbackLogoUrl = "data:image/svg+xml;base64,ZmFrZQ==";

    [Fact]
    public async Task ShopOverview_ShowsCollapsedRoadmapByDefault_WhenSetupIsComplete()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "overview");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Onboarding roadmap", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Show setup", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Next best step:", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DefaultWorkspaceRoute_LandsInShopProgrammesOperate()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId);

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Stamp visits", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Open Customer Join", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Programmes", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Run daily loyalty from here", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Operate for daily use", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShopOverview_DismissesRoadmapWhenRequested()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "overview");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        cut.FindAll("button")
            .Single(button => button.TextContent.Contains("Dismiss", StringComparison.Ordinal))
            .Click();

        Assert.DoesNotContain("Onboarding roadmap", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShopOverview_PresentsProgrammesAsChildSection_NotPeerWorkspaceScope()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "overview");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Programmes in this shop", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Daily use handoff", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Open programmes", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PrimaryNavigation_DoesNotExposeEditShopAsPeerTab()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "overview");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        var topLevelLinks = cut.FindAll("nav[aria-label='Shop sections'] a")
            .Select(anchor => anchor.TextContent.Trim())
            .ToArray();

        Assert.Contains("Overview", topLevelLinks);
        Assert.Contains("Programmes", topLevelLinks);
        Assert.Contains("Insights", topLevelLinks);
        Assert.DoesNotContain("Edit Shop", topLevelLinks);
        Assert.Contains("Shop settings", cut.Markup, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(cut.Markup, "Shop settings"));
    }

    [Fact]
    public async Task LegacyEditRoute_OpensShopSettingsDrawer()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "edit");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Save Shop", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Shop settings drawer", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Close", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Open Customer Join", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShopSettingsDrawer_CanOpenFromProgrammesWithoutLeavingProgrammeSurface()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "configure", settings: "shop");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Shop settings drawer", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Programme configuration", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Wallet loyalty card", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammesConfigure_ShowsProgrammeFormAndNotCustomerJoinAction()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "configure");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Save Programme", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Customer delivery", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("wallet loyalty card", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Current live output for this programme.", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Open Customer Join", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Current output and future room", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Choose how customers receive this programme", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Programmes_NewProgrammeFlow_MakesWalletDeliveryExplicit()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "operate");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        cut.FindAll("button")
            .Single(button => button.TextContent.Contains("New programme", StringComparison.Ordinal))
            .Click();

        Assert.Contains("Choose output", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Wallet loyalty card", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Launch format", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Live now", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammesRail_ReadsAsNavigationNotExplainer()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "operate");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Programmes", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("A programme defines the rule", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Run daily loyalty from here", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammeContextBar_IsCompactAndDoesNotExplainTabs()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "configure");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Current programme", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Wallet loyalty card", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Operate for daily use", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("0 customers", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("0 unlocked", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammesOperate_UsesLeanOperateSurface()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "operate");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Customer join QR", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Stamp visits", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Issued on this programme", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Programme timeline", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Programme specific", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Selected programme timeline", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammesConfigure_UsesCalmerPreviewLanguage()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "configure");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Wallet loyalty card preview", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Current delivery", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Uses the shop brand defaults for this programme.", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Customer-facing view", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammesOperate_DisablesJoinAction_WhenProgrammeIsScheduled()
    {
        var state = CreateState();
        using var context = CreateContext();
        RegisterServices(context, state);
        var workspace = await CreateMerchantAsync(state);
        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(5);
        var endsOn = startsOn.AddDays(30);

        var updated = state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            workspace.SelectedProgramme.ProgrammeId,
            workspace.SelectedProgramme.RewardItemLabel,
            workspace.SelectedProgramme.RewardThreshold,
            workspace.SelectedProgramme.RewardCopy,
            startsOn,
            endsOn,
            BaseUri);
        Assert.NotNull(updated);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "operate", programmeId: updated!.SelectedProgramme.ProgrammeId);

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains($"This programme opens on {startsOn:dd MMM yyyy}.", cut.Markup, StringComparison.Ordinal);

        var disabledJoinButton = cut.FindAll("button")
            .Single(button => button.TextContent.Contains("Open Customer Join", StringComparison.Ordinal));

        Assert.True(disabledJoinButton.HasAttribute("disabled"));
    }

    [Fact]
    public async Task ProgrammesOperate_UsesSelectedProgrammeContext()
    {
        var state = CreateState();
        using var context = CreateContext();
        RegisterServices(context, state);
        var workspace = await CreateMerchantAsync(state);
        var secondProgramme = state.CreateProgramme(workspace.Merchant.MerchantId, BaseUri);
        Assert.NotNull(secondProgramme);

        var updatedProgramme = state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            secondProgramme!.SelectedProgramme.ProgrammeId,
            "espressos",
            7,
            "Buy 7 espressos, get one free.",
            secondProgramme.SelectedProgramme.StartsOn,
            secondProgramme.SelectedProgramme.EndsOn,
            BaseUri);

        Assert.NotNull(updatedProgramme);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "operate", programmeId: updatedProgramme!.SelectedProgramme.ProgrammeId);

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains(updatedProgramme.SelectedProgramme.JoinCode, cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain(workspace.SelectedProgramme.JoinCode, cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammesScope_GroupsProgrammesByLifecycle()
    {
        var state = CreateState();
        using var context = CreateContext();
        RegisterServices(context, state);
        var workspace = await CreateMerchantAsync(state);

        var scheduled = state.CreateProgramme(workspace.Merchant.MerchantId, BaseUri);
        Assert.NotNull(scheduled);
        var scheduledStartsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(7);
        var scheduledEndsOn = scheduledStartsOn.AddDays(30);
        Assert.NotNull(state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            scheduled!.SelectedProgramme.ProgrammeId,
            scheduled.SelectedProgramme.RewardItemLabel,
            scheduled.SelectedProgramme.RewardThreshold,
            scheduled.SelectedProgramme.RewardCopy,
            scheduledStartsOn,
            scheduledEndsOn,
            BaseUri));

        var expired = state.CreateProgramme(workspace.Merchant.MerchantId, BaseUri);
        Assert.NotNull(expired);
        var expiredStartsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-60);
        var expiredEndsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-7);
        Assert.NotNull(state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            expired!.SelectedProgramme.ProgrammeId,
            expired.SelectedProgramme.RewardItemLabel,
            expired.SelectedProgramme.RewardThreshold,
            expired.SelectedProgramme.RewardCopy,
            expiredStartsOn,
            expiredEndsOn,
            BaseUri));

        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "operate");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains(">Active<", cut.Markup, StringComparison.Ordinal);
        Assert.Contains(">Scheduled<", cut.Markup, StringComparison.Ordinal);
        Assert.Contains(">Expired<", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammeSelection_PreservesCurrentProgrammeSection()
    {
        var state = CreateState();
        using var context = CreateContext();
        RegisterServices(context, state);
        var workspace = await CreateMerchantAsync(state);
        var secondProgramme = state.CreateProgramme(workspace.Merchant.MerchantId, BaseUri);
        Assert.NotNull(secondProgramme);

        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "insights");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        var matchingLink = cut.FindAll("a")
            .Any(anchor =>
            {
                var href = anchor.GetAttribute("href");
                return href is not null
                    && href.Contains("section=programmes", StringComparison.Ordinal)
                    && href.Contains("programmeSection=insights", StringComparison.Ordinal)
                    && href.Contains($"programme={secondProgramme!.SelectedProgramme.ProgrammeId}", StringComparison.Ordinal);
            });

        Assert.True(matchingLink);
    }

    [Fact]
    public async Task ShopInsights_ShowsShopLevelMetricsOnly()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "insights");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Across every programme in the shop", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Programme insights", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammesInsights_ShowsSelectedProgrammeMetrics()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "insights");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Programme insights", cut.Markup, StringComparison.Ordinal);
        Assert.Contains(workspace.SelectedProgramme.JoinCode, cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Across every programme in the shop", cut.Markup, StringComparison.Ordinal);
    }

    private static BunitContext CreateContext() => new();

    private async Task<MerchantWorkspaceSnapshot> CreateMerchantAndRegisterServicesAsync(BunitContext context)
    {
        var state = CreateState();
        RegisterServices(context, state);
        return await CreateMerchantAsync(state);
    }

    private static void RegisterServices(BunitContext context, DemoPlatformState state)
    {
        context.Services.AddSingleton(state);
        context.Services.AddSingleton(new JoinQrCodeService());
        context.Services.AddSingleton(new MerchantBrandPresentationService());
        context.Services.AddSingleton<IAppleWalletPassService>(new FakeAppleWalletPassService());
    }

    private static void NavigateToWorkspace(
        BunitContext context,
        Guid merchantId,
        string? section = null,
        string? programmeSection = null,
        Guid? programmeId = null,
        string? legacyTab = null,
        string? settings = null)
    {
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        var queryParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(legacyTab))
        {
            queryParts.Add($"tab={legacyTab}");
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            queryParts.Add($"section={section}");
        }

        if (!string.IsNullOrWhiteSpace(programmeSection))
        {
            queryParts.Add($"programmeSection={programmeSection}");
        }

        if (programmeId.HasValue)
        {
            queryParts.Add($"programme={programmeId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(settings))
        {
            queryParts.Add($"settings={settings}");
        }

        var query = queryParts.Count == 0 ? string.Empty : $"?{string.Join("&", queryParts)}";
        navigation.NavigateTo($"/portal/merchant/{merchantId}{query}");
    }

    private static DemoPlatformState CreateState() =>
        new(NullLogger<DemoPlatformState>.Instance, new InMemoryMerchantBrandAssetStore());

    private static int CountOccurrences(string value, string search) =>
        value.Split(search, StringSplitOptions.None).Length - 1;

    private static Task<MerchantWorkspaceSnapshot> CreateMerchantAsync(DemoPlatformState state) =>
        state.CreateMerchantAsync(
            "Jo's Coffee",
            "Bristol",
            "BS1 4DJ",
            "owner@joscoffee.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: "#1f3731",
            accentColor: "#f4c15d",
            baseUri: BaseUri);

    private sealed class FakeAppleWalletPassService : IAppleWalletPassService
    {
        public WalletPassCapability GetCapability() =>
            new(
                WalletPassDeliveryMode.Preview,
                "Open Card Preview",
                "Preview only for component tests.");

        public Task<WalletPassPackage> CreatePackageAsync(WalletCardSnapshot card, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Component tests do not issue Apple Wallet packages.");
    }

    private sealed class InMemoryMerchantBrandAssetStore : Fic.MerchantAccounts.IMerchantBrandAssetStore
    {
        public Task<string> SaveLogoAsync(Guid merchantId, Fic.MerchantAccounts.MerchantLogoUpload upload, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Fic.MerchantAccounts.MerchantBrandAssetInspector.EnsureValidPng(upload.Bytes);
            return Task.FromResult($"{Fic.MerchantAccounts.MerchantBrandAssetDefaults.PublicRequestPath}/{merchantId:N}/logo.png");
        }

        public Task<Fic.MerchantAccounts.MerchantBrandAsset?> GetAssetAsync(string publicPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Fic.MerchantAccounts.MerchantBrandAsset?>(null);
        }
    }
}
