using Fic.Contracts;
using Fic.MerchantAccounts;
using Fic.Platform.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Fic.Platform.Web.Tests;

public sealed class DemoPlatformStateTests
{
    private const string BaseUri = "https://demo.fic.test/";
    private const string DefaultPrimaryColor = "#1f3731";
    private const string DefaultAccentColor = "#f4c15d";
    private const string FallbackLogoUrl = "data:image/svg+xml;base64,ZmFrZQ==";

    [Fact]
    public async Task CreateMerchantAsync_CreatesShopWithoutProgrammeUntilProgrammeIsConfigured()
    {
        var state = CreateState();

        var workspace = await CreateMerchantAsync(state);

        Assert.Empty(workspace.Programmes);
        Assert.Null(workspace.SelectedProgramme);
        Assert.False(workspace.SetupChecklist.HasAnyProgramme);
        Assert.Null(workspace.SelectedJoinUrl);
    }

    [Fact]
    public async Task CreateProgramme_FirstProgrammeUsesTemplateAndOneYearWindow()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);

        var updated = state.CreateProgramme(workspace.Merchant.MerchantId, "coffee-visits", BaseUri);

        Assert.NotNull(updated);
        Assert.Single(updated!.Programmes);
        var selectedProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(updated.SelectedProgramme);
        Assert.Equal("coffee-visits", selectedProgramme.TemplateKey);
        Assert.Equal("Coffee stamp card", selectedProgramme.TemplateLabel);
        Assert.Equal("visit-reward", selectedProgramme.ProgrammeTypeKey);
        Assert.Equal("Visit reward", selectedProgramme.ProgrammeTypeLabel);
        Assert.Equal("apple-wallet-pass", selectedProgramme.DeliveryTypeKey);
        Assert.Equal("Apple Wallet pass", selectedProgramme.DeliveryTypeLabel);
        Assert.Equal("Wallet loyalty card", selectedProgramme.OutputLabel);
        Assert.Equal(selectedProgramme.StartsOn.AddDays(365), selectedProgramme.EndsOn);
        Assert.Contains(selectedProgramme.JoinCode, updated.SelectedJoinUrl, StringComparison.Ordinal);
    }

    [Fact]
    public void GetProgrammeTemplates_ExposesVisitAndFoodOfferStarters()
    {
        var state = CreateState();

        var templates = state.GetProgrammeTemplates();

        Assert.Contains(templates, template => template.TemplateKey == "coffee-visits");
        Assert.Contains(templates, template => template.TemplateKey == "coffee-food-offer");
        Assert.Contains(templates, template => template.ProgrammeTypeLabel == "Visit reward");
        Assert.Contains(templates, template => template.ProgrammeTypeLabel == "Conditional offer");
        Assert.All(templates, template => Assert.Equal("Apple Wallet pass", template.DeliveryTypeLabel));
    }

    [Fact]
    public async Task UpdateProgramme_RejectsExpiryBeforeBegin()
    {
        var state = CreateState();
        var workspace = await CreateMerchantWithProgrammeAsync(state);
        var selectedProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(workspace.SelectedProgramme);
        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(10);
        var endsOn = startsOn.AddDays(-1);

        var error = Assert.Throws<InvalidOperationException>(() =>
            state.UpdateProgramme(
                workspace.Merchant.MerchantId,
                selectedProgramme.ProgrammeId,
                selectedProgramme.RewardItemLabel,
                selectedProgramme.RewardThreshold,
                selectedProgramme.RewardCopy,
                startsOn,
                endsOn,
                BaseUri));

        Assert.Equal("Expiry date must be on or after the begin date.", error.Message);
    }

    [Fact]
    public async Task JoinCustomer_ReturnsNull_WhenProgrammeHasNotStarted()
    {
        var state = CreateState();
        var workspace = await CreateMerchantWithProgrammeAsync(state);
        var selectedProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(workspace.SelectedProgramme);
        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(7);
        var endsOn = startsOn.AddDays(30);

        var updated = state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            selectedProgramme.ProgrammeId,
            selectedProgramme.RewardItemLabel,
            selectedProgramme.RewardThreshold,
            selectedProgramme.RewardCopy,
            startsOn,
            endsOn,
            BaseUri);

        var updatedProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(updated!.SelectedProgramme);
        var joined = state.JoinCustomer(updatedProgramme.JoinCode);

        Assert.Null(joined);
    }

    [Fact]
    public async Task AwardVisit_ReturnsNull_WhenProgrammeHasExpired()
    {
        var state = CreateState();
        var workspace = await CreateMerchantWithProgrammeAsync(state);
        var selectedProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(workspace.SelectedProgramme);
        var joined = state.JoinCustomer(selectedProgramme.JoinCode);
        Assert.NotNull(joined);

        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-30);
        var endsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-1);
        var expired = state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            selectedProgramme.ProgrammeId,
            selectedProgramme.RewardItemLabel,
            selectedProgramme.RewardThreshold,
            selectedProgramme.RewardCopy,
            startsOn,
            endsOn,
            BaseUri);

        var expiredProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(expired!.SelectedProgramme);
        var result = state.AwardVisit(
            workspace.Merchant.MerchantId,
            expiredProgramme.ProgrammeId,
            joined!.CardCode,
            BaseUri);

        Assert.Null(result);
    }

    [Fact]
    public async Task WalletCardSnapshot_TracksUpdatedProgrammeDates()
    {
        var state = CreateState();
        var workspace = await CreateMerchantWithProgrammeAsync(state);
        var selectedProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(workspace.SelectedProgramme);
        var joined = state.JoinCustomer(selectedProgramme.JoinCode);
        Assert.NotNull(joined);

        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-2);
        var endsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(14);

        state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            selectedProgramme.ProgrammeId,
            "espressos",
            7,
            "Buy 7 espressos, get one free.",
            startsOn,
            endsOn,
            BaseUri);

        var walletCard = state.GetWalletCard(joined!.CardId);

        Assert.NotNull(walletCard);
        Assert.Equal(startsOn, walletCard!.StartsOn);
        Assert.Equal(endsOn, walletCard.EndsOn);
        Assert.Equal("espressos", walletCard.RewardItemLabel);
        Assert.Equal(7, walletCard.TargetCount);
    }

    [Fact]
    public async Task ConfigureMerchantAccess_StoresCredentials_AndAllowsAuthentication()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);

        var configured = state.ConfigureMerchantAccess(workspace.Merchant.MerchantId, "very-secure-password");
        var authenticated = state.AuthenticateMerchant(workspace.Merchant.ContactEmail, "very-secure-password");

        Assert.Equal(MerchantCredentialConfigurationStatus.Updated, configured.Status);
        Assert.Equal(MerchantAuthenticationStatus.Authenticated, authenticated.Status);
        Assert.Equal(workspace.Merchant.MerchantId, authenticated.Merchant!.MerchantId);
    }

    [Fact]
    public async Task AuthenticateMerchant_ReturnsCredentialsNotConfigured_WhenPasswordHasNotBeenSet()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);

        var authenticated = state.AuthenticateMerchant(workspace.Merchant.ContactEmail, "password");

        Assert.Equal(MerchantAuthenticationStatus.CredentialsNotConfigured, authenticated.Status);
        Assert.Null(authenticated.Merchant);
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
            primaryColor: DefaultPrimaryColor,
            accentColor: DefaultAccentColor,
            baseUri: BaseUri);

    private static async Task<MerchantWorkspaceSnapshot> CreateMerchantWithProgrammeAsync(DemoPlatformState state, string templateKey = "coffee-visits")
    {
        var workspace = await CreateMerchantAsync(state);
        return state.CreateProgramme(workspace.Merchant.MerchantId, templateKey, BaseUri)!;
    }

    private sealed class InMemoryMerchantBrandAssetStore : IMerchantBrandAssetStore
    {
        private readonly Dictionary<string, MerchantBrandAsset> _assets = new(StringComparer.OrdinalIgnoreCase);

        public Task<string> SaveLogoAsync(Guid merchantId, MerchantLogoUpload upload, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            MerchantBrandAssetInspector.EnsureValidPng(upload.Bytes);

            var path = $"{MerchantBrandAssetDefaults.PublicRequestPath}/{merchantId:N}/logo.png";
            _assets[path] = new MerchantBrandAsset(upload.Bytes, upload.ContentType);
            return Task.FromResult(path);
        }

        public Task<MerchantBrandAsset?> GetAssetAsync(string publicPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _assets.TryGetValue(publicPath, out var asset);
            return Task.FromResult(asset);
        }
    }
}
