using Bunit;
using Fic.Platform.Web.Components.Layout;
using Fic.Platform.Web.Components.Pages;
using Fic.Platform.Web.Services;
using Fic.WalletPasses;
using Microsoft.AspNetCore.Components;
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

        Assert.Contains("North Star Customer Solutions", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Set up your wallet loyalty programme in minutes.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Set Up Your Shop", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Already have an account?", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/portal/signup", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/consultancy", cut.Markup, StringComparison.Ordinal);
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
        Assert.Contains("Billing and owner password setup are next", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Step 1 of 3", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Continue to Billing", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Next: confirm mock billing", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BillingPage_ReadsAsFinalOnboardingStep()
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

        Assert.Contains("Set the owner password here", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Continue to first programme", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Owner password", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Founding coffee shop", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("£19.99/mo", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("launch=create", cut.Markup, StringComparison.Ordinal);
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

        Assert.Contains("Support by North Star", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/support/wallet-demo", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/consultancy", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/support/billing", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/support/account", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/account/logout", cut.Markup, StringComparison.Ordinal);
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
