using Bunit;
using Fic.Platform.Web.Components.Layout;
using Fic.Platform.Web.Components.Pages;
using Fic.Platform.Web.Services;
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
        Assert.Contains("Designing loyalty that actually works.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/portal/signup", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/consultancy", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("company-logo.png", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void AccountSurfaces_ReadAsCompanySupport()
    {
        using var context = new BunitContext();

        var login = context.Render<Login>();
        Assert.Contains("North Star account support", login.Markup, StringComparison.Ordinal);
        Assert.Contains("Log in to your FIC workspace", login.Markup, StringComparison.Ordinal);

        var forgotPassword = context.Render<ForgotPassword>();
        Assert.Contains("North Star account support", forgotPassword.Markup, StringComparison.Ordinal);
        Assert.Contains("Reset your password", forgotPassword.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ConsultancyAndSupportPages_ExistAsBrandedCompanySurfaces()
    {
        using var context = new BunitContext();

        var consultancy = context.Render<Consultancy>();
        Assert.Contains("Training &amp; Consultancy", consultancy.Markup, StringComparison.Ordinal);
        Assert.Contains("North Star Customer Solutions", consultancy.Markup, StringComparison.Ordinal);

        var billing = context.Render<SupportBilling>();
        Assert.Contains("Billing and commercial help", billing.Markup, StringComparison.Ordinal);

        var account = context.Render<SupportAccount>();
        Assert.Contains("Account and access help", account.Markup, StringComparison.Ordinal);
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
        Assert.Contains("/consultancy", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/support/billing", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/support/account", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("/account/login", cut.Markup, StringComparison.Ordinal);
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
