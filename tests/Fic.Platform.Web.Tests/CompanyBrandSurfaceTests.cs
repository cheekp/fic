using Bunit;
using Fic.Platform.Web.Components.Layout;
using Fic.Platform.Web.Components.Pages;
using Fic.Platform.Web.Services;
using Fic.WalletPasses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Fic.Platform.Web.Tests;

public sealed class CompanyBrandSurfaceTests
{
    private const string BaseUri = "https://demo.fic.test/";
    private const string FallbackLogoUrl = "data:image/svg+xml;base64,ZmFrZQ==";

    [Fact]
    public void HomePage_UsesNorthStarPositioning_AndKeepsClearSignupPath()
    {
        using var context = new BunitContext();

        var cut = context.Render<Home>();

        Assert.Contains("Set up your Apple Wallet loyalty programme in minutes.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("North Star loyalty platform", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/images/home-hero.jpeg", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Sign up now", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Merchant log in", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Apple Wallet first", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/portal/signup", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/account/login", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/images/social/instagram.svg", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("https://www.facebook.com", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("https://www.tiktok.com", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("https://x.com", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("I already have an account", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain(">FIC<", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Consultancy", cut.Find("nav[aria-label='Company links']").TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("company-logo.png", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void SignupPage_ReadsAsShortShopCreationStep()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new DemoPlatformState(NullLogger<DemoPlatformState>.Instance, new InMemoryMerchantBrandAssetStore()));

        var cut = context.Render<PortalSignup>();

        Assert.Contains("Create your shop", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Next, choose a plan, set billing and owner access", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Step 1 of 5", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Continue to Plan", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Onboarding journey", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("form-actions--onboarding", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("button--wide", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("owner@joscoffee.test", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Jo&#x27;s Coffee", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Use demo details", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Town or city", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Postcode", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Next: confirm mock billing", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void SignupPage_ShowsDemoSeedAction_WhenFeatureFlagIsEnabled()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(new DemoPlatformState(NullLogger<DemoPlatformState>.Instance, new InMemoryMerchantBrandAssetStore()));
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:SignupDemoSeedEnabled"] = "true"
            })
            .Build();
        context.Services.AddSingleton<IConfiguration>(config);

        var cut = context.Render<PortalSignup>();

        Assert.Contains("Use demo details", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PlanPage_ReadsAsTierSelectionStep()
    {
        using var context = new BunitContext();
        var state = new DemoPlatformState(NullLogger<DemoPlatformState>.Instance, new InMemoryMerchantBrandAssetStore());
        context.Services.AddSingleton(state);

        var workspace = await state.CreateMerchantAsync(
            "Jo's Coffee",
            "Bristol",
            "BS1 4DJ",
            "owner@joscoffee.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: "#1f3731",
            accentColor: "#f4c15d",
            baseUri: BaseUri);

        var cut = context.Render<SignupPlan>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Choose your launch plan", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Step 2 of 5", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Continue with Starter", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Starter", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Growth", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Enterprise", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("SSO and access governance", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("CRM integrations and data handoff", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Consultancy-led rollout", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("£79/mo", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Custom", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/consultancy", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Use demo plan", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Owner password", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Payment method", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BillingPage_ReadsAsPaymentAndOwnerAccessStep()
    {
        using var context = new BunitContext();
        var state = new DemoPlatformState(NullLogger<DemoPlatformState>.Instance, new InMemoryMerchantBrandAssetStore());
        context.Services.AddSingleton(state);

        var workspace = await state.CreateMerchantAsync(
            "Jo's Coffee",
            "Bristol",
            "BS1 4DJ",
            "owner@joscoffee.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: "#1f3731",
            accentColor: "#f4c15d",
            baseUri: BaseUri);

        var cut = context.Render<SignupBilling>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Set payment and owner access", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Step 3 of 5", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Continue to shop details", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Owner password", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Payment method", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Apple Pay", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("apple-pay-mark.svg", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Selected plan", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Starter", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Change plan", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("form-actions--onboarding", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("button--wide", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Use demo payment details", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Growth", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Enterprise", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Card number", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("launch=create", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PlanAndBillingPages_ShowDemoSeedActions_WhenFeatureFlagIsEnabled()
    {
        using var context = new BunitContext();
        var state = new DemoPlatformState(NullLogger<DemoPlatformState>.Instance, new InMemoryMerchantBrandAssetStore());
        context.Services.AddSingleton(state);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:SignupDemoSeedEnabled"] = "true"
            })
            .Build();
        context.Services.AddSingleton<IConfiguration>(config);

        var workspace = await state.CreateMerchantAsync(
            "Jo's Coffee",
            "Bristol",
            "BS1 4DJ",
            "owner@joscoffee.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: "#1f3731",
            accentColor: "#f4c15d",
            baseUri: BaseUri);

        var plan = context.Render<SignupPlan>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId));
        var billing = context.Render<SignupBilling>(parameters => parameters
            .Add(page => page.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Use demo plan", plan.Markup, StringComparison.Ordinal);
        Assert.Contains("Use demo payment details", billing.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void AccountSurfaces_ReadAsCompanySupport()
    {
        using var context = new BunitContext();

        var login = context.Render<Login>();
        Assert.Contains("North Star account support", login.Markup, StringComparison.Ordinal);
        Assert.Contains("Log in to your FIC workspace", login.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Authentication is still a stub", login.Markup, StringComparison.Ordinal);

        var forgotPassword = context.Render<ForgotPassword>();
        Assert.Contains("North Star account support", forgotPassword.Markup, StringComparison.Ordinal);
        Assert.Contains("Reset your password", forgotPassword.Markup, StringComparison.Ordinal);

        var accessDenied = context.Render<AccessDenied>();
        Assert.Contains("You do not have access to that workspace", accessDenied.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ConsultancyAndSupportPages_ExistAsBrandedCompanySurfaces()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IAppleWalletPassService>(new FakeAppleWalletPassService());
        context.Services.AddSingleton<IWalletPassUpdateNotifier>(new FakeWalletPassUpdateNotifier());

        var consultancy = context.Render<Consultancy>();
        Assert.Contains("Training &amp; Consultancy", consultancy.Markup, StringComparison.Ordinal);
        Assert.Contains("North Star Customer Solutions", consultancy.Markup, StringComparison.Ordinal);

        var billing = context.Render<SupportBilling>();
        Assert.Contains("Billing and commercial help", billing.Markup, StringComparison.Ordinal);

        var account = context.Render<SupportAccount>();
        Assert.Contains("Account and access help", account.Markup, StringComparison.Ordinal);

        var walletDemo = context.Render<SupportWalletDemo>();
        Assert.Contains("Apple Wallet demo readiness", walletDemo.Markup, StringComparison.Ordinal);
        Assert.Contains("Pass issuance", walletDemo.Markup, StringComparison.Ordinal);
        Assert.Contains("Pass refresh", walletDemo.Markup, StringComparison.Ordinal);
        Assert.Contains("Preview", walletDemo.Markup, StringComparison.Ordinal);
        Assert.Contains("Setup", walletDemo.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MerchantUtilityChrome_LinksToCompanySupportDestinations()
    {
        using var context = new BunitContext();
        var state = new DemoPlatformState(NullLogger<DemoPlatformState>.Instance, new InMemoryMerchantBrandAssetStore());
        context.Services.AddSingleton(state);
        context.Services.AddSingleton(new MerchantBrandPresentationService());

        var workspace = await state.CreateMerchantAsync(
            "Jo's Coffee",
            "Bristol",
            "BS1 4DJ",
            "owner@joscoffee.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: "#1f3731",
            accentColor: "#f4c15d",
            baseUri: BaseUri);

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"/portal/merchant/{workspace.Merchant.MerchantId}");

        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(layout => layout.Body, (RenderFragment)(_ => { })));

        Assert.Contains("/support/wallet-demo", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/consultancy", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/support/billing", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/support/account", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/account/logout", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Support by North Star", cut.Markup, StringComparison.Ordinal);
    }

    private sealed class FakeAppleWalletPassService : IAppleWalletPassService
    {
        public WalletPassCapability GetCapability() =>
            new(
                WalletPassDeliveryMode.Preview,
                "Open Card Preview",
                "Finish the Apple Wallet signing checklist to issue a real pass.",
                ["Signing certificate path is missing."]);

        public Task<WalletPassPackage> CreatePackageAsync(
            Fic.Contracts.WalletCardSnapshot card,
            string authenticationToken,
            string webServiceUrl,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Company support tests do not issue Apple Wallet packages.");
    }

    private sealed class FakeWalletPassUpdateNotifier : IWalletPassUpdateNotifier
    {
        public WalletPassPushCapability GetCapability() =>
            new(
                false,
                "Wallet refresh push delivery is turned off, so pass updates rely on manual refresh behavior.",
                ["Wallet refresh push delivery is turned off for this environment."]);

        public Task<WalletPassUpdateDispatchResult> NotifyPassUpdatedAsync(
            Fic.Contracts.WalletCardSnapshot card,
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
