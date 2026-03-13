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
    public async Task ShopTab_ShowsSetupChecklist()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, "shop");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Setup checklist", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Completed setup", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CardsTab_DoesNotShowCustomerJoinAction()
    {
        using var context = CreateContext();
        var workspace = await CreateMerchantAndRegisterServicesAsync(context);
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, "cards");

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains("Save Loyalty Card", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Open Customer Join", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CustomersTab_DisablesJoinAction_WhenProgrammeIsScheduled()
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
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, "customers", updated!.SelectedProgramme.ProgrammeId);

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains($"This loyalty card opens on {startsOn:dd MMM yyyy}.", cut.Markup, StringComparison.Ordinal);

        var disabledJoinButton = cut.FindAll("button")
            .Single(button => button.TextContent.Contains("Open Customer Join", StringComparison.Ordinal));

        Assert.True(disabledJoinButton.HasAttribute("disabled"));
    }

    [Fact]
    public async Task CustomersTab_UsesSelectedProgrammeContext()
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
        NavigateToWorkspace(context, workspace.Merchant.MerchantId, "customers", updatedProgramme!.SelectedProgramme.ProgrammeId);

        var cut = context.Render<VendorWorkspace>(parameters => parameters
            .Add(p => p.MerchantId, workspace.Merchant.MerchantId));

        Assert.Contains(updatedProgramme.SelectedProgramme.JoinCode, cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain(workspace.SelectedProgramme.JoinCode, cut.Markup, StringComparison.Ordinal);
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

    private static void NavigateToWorkspace(BunitContext context, Guid merchantId, string tab, Guid? programmeId = null)
    {
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        var query = programmeId.HasValue
            ? $"?tab={tab}&programme={programmeId.Value}"
            : $"?tab={tab}";

        navigation.NavigateTo($"/portal/merchant/{merchantId}{query}");
    }

    private static DemoPlatformState CreateState() =>
        new(NullLogger<DemoPlatformState>.Instance, new InMemoryMerchantBrandAssetStore());

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

        public Task<WalletPassPackage> CreatePackageAsync(Fic.Contracts.WalletCardSnapshot card, CancellationToken cancellationToken = default) =>
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
