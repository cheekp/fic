using Bunit;
using Fic.Contracts;
using Fic.Platform.Web.Components.Pages;
using Fic.Platform.Web.Services;
using Fic.WalletPasses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Playwright;
using Xunit;

namespace Fic.Platform.Web.Tests;

public sealed class UxQualityGateTests
{
    [Fact]
    public async Task SignupSurfaces_UseSharedOnboardingShellContract()
    {
        var state = UxTestFixture.CreateState();
        using var context = UxTestFixture.CreateContext();
        UxTestFixture.RegisterServices(context, state);
        var workspace = await UxTestFixture.CreateMerchantAsync(state);

        var signup = context.Render<PortalSignup>();
        var plan = context.Render<SignupPlan>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId));
        var billing = context.Render<SignupBilling>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId));

        Assert.NotNull(signup.Find(".onboarding-flow"));
        Assert.NotNull(signup.Find(".onboarding-flow__journey .onboarding-journey"));
        Assert.NotNull(signup.Find(".onboarding-flow__panel.onboarding-step"));

        Assert.NotNull(plan.Find(".onboarding-flow"));
        Assert.NotNull(plan.Find(".onboarding-flow__journey .onboarding-journey"));
        Assert.NotNull(plan.Find(".onboarding-flow__panel.onboarding-step"));

        Assert.NotNull(billing.Find(".onboarding-flow"));
        Assert.NotNull(billing.Find(".onboarding-flow__journey .onboarding-journey"));
        Assert.NotNull(billing.Find(".onboarding-flow__panel.onboarding-step"));
    }

    [Fact]
    public async Task WorkspaceTabs_UseSharedSegmentedControlContract()
    {
        var state = UxTestFixture.CreateState();
        using var context = UxTestFixture.CreateContext();
        UxTestFixture.RegisterServices(context, state);
        var workspace = await UxTestFixture.CreateMerchantWithProgrammeAsync(state);

        UxTestFixture.NavigateToWorkspace(
            context,
            workspace.Merchant.MerchantId,
            section: "programmes",
            programmeSection: "configure");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId));

        var shopTabs = cut.Find("nav[aria-label='Shop sections']");
        var programmeTabs = cut.Find("nav[aria-label='Programme sections']");

        Assert.Contains("segmented-control", shopTabs.GetAttribute("class"), StringComparison.Ordinal);
        Assert.Contains("segmented-control", programmeTabs.GetAttribute("class"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProgrammeRail_NewProgrammeAction_RemainsSecondaryAndCompact()
    {
        var state = UxTestFixture.CreateState();
        using var context = UxTestFixture.CreateContext();
        UxTestFixture.RegisterServices(context, state);
        var workspace = await UxTestFixture.CreateMerchantWithProgrammeAsync(state);

        UxTestFixture.NavigateToWorkspace(
            context,
            workspace.Merchant.MerchantId,
            section: "programmes",
            programmeSection: "operate");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId));

        var newProgrammeLink = cut.Find("a.programme-rail__new-button");
        var classes = newProgrammeLink.GetAttribute("class") ?? string.Empty;
        var controls = cut.Find(".programme-rail__controls");
        var header = cut.Find(".programme-rail__header");

        Assert.Contains("button--secondary", classes, StringComparison.Ordinal);
        Assert.Contains("button--compact", classes, StringComparison.Ordinal);
        Assert.DoesNotContain("button--primary", classes, StringComparison.Ordinal);
        Assert.NotNull(controls.QuerySelector("a.programme-rail__new-button"));
        Assert.Empty(header.QuerySelectorAll("a.programme-rail__new-button"));
    }

    [Fact]
    public async Task ProgrammeContextMeta_RemainsLowNoiseWithSingleStatusChip()
    {
        var state = UxTestFixture.CreateState();
        using var context = UxTestFixture.CreateContext();
        UxTestFixture.RegisterServices(context, state);
        var workspace = await UxTestFixture.CreateMerchantWithProgrammeAsync(state);

        UxTestFixture.NavigateToWorkspace(
            context,
            workspace.Merchant.MerchantId,
            section: "programmes",
            programmeSection: "configure");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId));

        var meta = cut.Find(".programme-context-bar__meta");
        var statusChips = meta.QuerySelectorAll(".chip");

        Assert.Single(statusChips);
        Assert.NotNull(meta.QuerySelector(".programme-context-bar__window"));
        Assert.NotNull(meta.QuerySelector(".programme-context-bar__descriptor"));
    }

    [Fact]
    public void AppCss_DeclaresControlTokensAndReducedMotionFallback()
    {
        var css = File.ReadAllText(UxTestFixture.ResolveRepoPath("src/Fic.Platform.Web/wwwroot/app.css"));

        Assert.Contains("--radius-control", css, StringComparison.Ordinal);
        Assert.Contains("--control-height", css, StringComparison.Ordinal);
        Assert.Contains("--signup-shell-width", css, StringComparison.Ordinal);
        Assert.Contains(".onboarding-flow", css, StringComparison.Ordinal);
        Assert.Contains(".onboarding-flow__journey", css, StringComparison.Ordinal);
        Assert.Contains(".programme-workpane--refresh", css, StringComparison.Ordinal);
        Assert.Contains(".programme-selection-strip--refresh", css, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", css, StringComparison.Ordinal);
    }
}

public sealed class UxBrowserSmokeTests
{
    [Fact]
    public async Task KeySurfaces_DoNotOverflowCoreViewports_WhenBrowserSmokeEnabled()
    {
        if (!UxTestFixture.IsBrowserSmokeEnabled())
        {
            return;
        }

        var css = File.ReadAllText(UxTestFixture.ResolveRepoPath("src/Fic.Platform.Web/wwwroot/app.css"));
        var homeMarkup = UxTestFixture.RenderHomeMarkup();
        var workspaceMarkup = await UxTestFixture.RenderWorkspaceMarkupAsync();

        var scenarios = new[]
        {
            new UxViewport("home-desktop", 1440, 900, homeMarkup),
            new UxViewport("home-mobile", 390, 844, homeMarkup),
            new UxViewport("workspace-desktop", 1440, 900, workspaceMarkup),
            new UxViewport("workspace-tablet", 1024, 768, workspaceMarkup)
        };

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        foreach (var scenario in scenarios)
        {
            await AssertNoHorizontalOverflowAsync(browser, css, scenario);
        }
    }

    private static async Task AssertNoHorizontalOverflowAsync(IBrowser browser, string css, UxViewport scenario)
    {
        var html = $$"""
                     <!doctype html>
                     <html>
                     <head>
                       <meta charset="utf-8" />
                       <meta name="viewport" content="width=device-width, initial-scale=1" />
                       <style>
                         html, body { margin: 0; padding: 0; }
                         *, *::before, *::after { box-sizing: border-box; }
                         {{css}}
                       </style>
                     </head>
                     <body>
                     {{scenario.Markup}}
                     </body>
                     </html>
                     """;

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = scenario.Width,
                Height = scenario.Height
            }
        });

        var page = await context.NewPageAsync();
        await page.SetContentAsync(html, new PageSetContentOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        var metrics = await page.EvaluateAsync<UxOverflowMetrics>(
            "() => ({ scrollWidth: document.documentElement.scrollWidth, viewportWidth: window.innerWidth })");

        Assert.NotNull(metrics);
        Assert.True(
            metrics!.ScrollWidth <= metrics.ViewportWidth + 1,
            $"Horizontal overflow detected for {scenario.Name}. scrollWidth={metrics.ScrollWidth} viewportWidth={metrics.ViewportWidth}");

        var artifactsDirectory = UxTestFixture.ResolveRepoPath("artifacts/ux-smoke");
        Directory.CreateDirectory(artifactsDirectory);

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(artifactsDirectory, $"{scenario.Name}.png"),
            FullPage = true
        });

        await context.CloseAsync();
    }

    private sealed record UxViewport(string Name, int Width, int Height, string Markup);

    private sealed class UxOverflowMetrics
    {
        public int ScrollWidth { get; set; }

        public int ViewportWidth { get; set; }
    }
}

internal static class UxTestFixture
{
    private const string BaseUri = "https://demo.fic.test/";
    private const string FallbackLogoUrl = "data:image/svg+xml;base64,ZmFrZQ==";

    public static bool IsBrowserSmokeEnabled() =>
        string.Equals(Environment.GetEnvironmentVariable("FIC_UX_BROWSER_SMOKE"), "1", StringComparison.Ordinal);

    public static BunitContext CreateContext() => new();

    public static DemoPlatformState CreateState() =>
        new(NullLogger<DemoPlatformState>.Instance, new InMemoryMerchantBrandAssetStore());

    public static void RegisterServices(BunitContext context, DemoPlatformState state)
    {
        context.Services.AddSingleton(state);
        context.Services.AddSingleton(new JoinQrCodeService());
        context.Services.AddSingleton(new MerchantBrandPresentationService());
        context.Services.AddSingleton<IAppleWalletPassService>(new FakeAppleWalletPassService());
        context.Services.AddSingleton<IWalletPassUpdateNotifier>(new FakeWalletPassUpdateNotifier());
    }

    public static string ResolveRepoPath(string relativePath) =>
        Path.Combine(FindRepoRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));

    public static async Task<MerchantWorkspaceSnapshot> CreateMerchantWithProgrammeAsync(DemoPlatformState state)
    {
        var workspace = await CreateMerchantAsync(state);

        return state.CreateProgramme(workspace.Merchant.MerchantId, "coffee-visits", BaseUri)!;
    }

    public static Task<MerchantWorkspaceSnapshot> CreateMerchantAsync(DemoPlatformState state) =>
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

    public static void NavigateToWorkspace(
        BunitContext context,
        Guid merchantId,
        string? section = null,
        string? programmeSection = null)
    {
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        var queryParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(section))
        {
            queryParts.Add($"section={section}");
        }

        if (!string.IsNullOrWhiteSpace(programmeSection))
        {
            queryParts.Add($"programmeSection={programmeSection}");
        }

        var query = queryParts.Count == 0 ? string.Empty : $"?{string.Join("&", queryParts)}";
        navigation.NavigateTo($"/portal/merchant/{merchantId}{query}");
    }

    public static string RenderHomeMarkup()
    {
        using var context = CreateContext();
        return context.Render<Home>().Markup;
    }

    public static async Task<string> RenderWorkspaceMarkupAsync()
    {
        var state = CreateState();
        using var context = CreateContext();
        RegisterServices(context, state);
        var workspace = await CreateMerchantWithProgrammeAsync(state);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, section: "programmes", programmeSection: "configure");
        return context.Render<VendorWorkspace>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId))
            .Markup;
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Fic.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing Fic.sln.");
    }

    private sealed class FakeAppleWalletPassService : IAppleWalletPassService
    {
        public WalletPassCapability GetCapability() =>
            new(
                WalletPassDeliveryMode.Preview,
                "Open Card Preview",
                "Preview only for UX contract tests.");

        public Task<WalletPassPackage> CreatePackageAsync(
            WalletCardSnapshot card,
            string authenticationToken,
            string webServiceUrl,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("UX contract tests do not issue Apple Wallet packages.");
    }

    private sealed class FakeWalletPassUpdateNotifier : IWalletPassUpdateNotifier
    {
        public WalletPassPushCapability GetCapability() =>
            new(false, "Wallet refresh push delivery is turned off for this environment.");

        public Task<WalletPassUpdateDispatchResult> NotifyPassUpdatedAsync(
            WalletCardSnapshot card,
            IReadOnlyList<string> pushTokens,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new WalletPassUpdateDispatchResult(
                pushTokens.Count,
                0,
                0,
                true,
                "Wallet refresh was not requested because push delivery is not configured."));
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
