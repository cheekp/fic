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
        Assert.Equal("wallet-loyalty-card", selectedProgramme.CardTypeKey);
        Assert.Equal("Wallet loyalty card", selectedProgramme.CardTypeLabel);
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
    public void GetShopTypes_ReturnsOnlyActiveShopTypes()
    {
        var state = CreateState(CreateVisibilityManagedCatalogue());

        var shopTypes = state.GetShopTypes();

        var shopType = Assert.Single(shopTypes);
        Assert.Equal("coffee", shopType.ShopTypeKey);
    }

    [Fact]
    public void GetCardTypes_ReturnsOnlyActiveCardTypes()
    {
        var state = CreateState(CreateVisibilityManagedCatalogue());

        var cardTypes = state.GetCardTypes();

        var cardType = Assert.Single(cardTypes);
        Assert.Equal("wallet-loyalty-card", cardType.CardTypeKey);
    }

    [Fact]
    public void GetProgrammeTemplates_HidesTemplates_WhenTemplateOrReferencedTypeIsInactive()
    {
        var state = CreateState(CreateVisibilityManagedCatalogue());

        var templates = state.GetProgrammeTemplates();

        var template = Assert.Single(templates);
        Assert.Equal("visible-template", template.TemplateKey);
        Assert.True(template.IsActive);
        Assert.Equal("coffee", template.ShopTypeKey);
        Assert.Equal("wallet-loyalty-card", template.CardTypeKey);
    }

    [Fact]
    public async Task CreateMerchantAsync_StoresSelectedShopType()
    {
        var state = CreateState();

        var workspace = await CreateMerchantAsync(state, shopTypeKey: "barber");

        Assert.Equal("barber", workspace.Merchant.ShopTypeKey);
    }

    [Fact]
    public async Task CreateProgramme_RejectsTemplate_WhenTemplateShopTypeDoesNotMatchMerchantShopType()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state, shopTypeKey: "barber");

        var rejected = state.CreateProgramme(workspace.Merchant.MerchantId, "coffee-visits", BaseUri);
        var accepted = state.CreateProgramme(workspace.Merchant.MerchantId, "barber-visit-loyalty", BaseUri);

        Assert.Null(rejected);
        Assert.NotNull(accepted);
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
    public async Task WalletCardSnapshot_DerivesRewardReadyAndRedeemedStatuses()
    {
        var state = CreateState();
        var workspace = await CreateMerchantWithProgrammeAsync(state);
        var selectedProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(workspace.SelectedProgramme);
        var joined = state.JoinCustomer(selectedProgramme.JoinCode);
        Assert.NotNull(joined);

        for (var i = 0; i < selectedProgramme.RewardThreshold; i++)
        {
            Assert.NotNull(state.AwardVisit(workspace.Merchant.MerchantId, selectedProgramme.ProgrammeId, joined!.CardCode, BaseUri));
        }

        var rewardReadyCard = state.GetWalletCard(joined!.CardId);
        Assert.NotNull(rewardReadyCard);
        Assert.Equal(CustomerCardStatus.RewardReady, rewardReadyCard!.CustomerCardStatus);
        Assert.Equal("Reward ready", rewardReadyCard.CustomerCardStatusLabel);
        Assert.True(rewardReadyCard.CanRedeem);

        Assert.NotNull(state.RedeemReward(workspace.Merchant.MerchantId, selectedProgramme.ProgrammeId, joined.CardId, BaseUri));

        var redeemedCard = state.GetWalletCard(joined.CardId);
        Assert.NotNull(redeemedCard);
        Assert.Equal(CustomerCardStatus.Redeemed, redeemedCard!.CustomerCardStatus);
        Assert.Equal("Redeemed", redeemedCard.CustomerCardStatusLabel);
        Assert.False(redeemedCard.CanRedeem);
    }

    [Fact]
    public async Task WalletCardSnapshot_DerivesScheduledAndExpiredStatusesFromProgrammeWindow()
    {
        var state = CreateState();
        var workspace = await CreateMerchantWithProgrammeAsync(state);
        var selectedProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(workspace.SelectedProgramme);
        var joined = state.JoinCustomer(selectedProgramme.JoinCode);
        Assert.NotNull(joined);

        var scheduledStartsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(10);
        var scheduledEndsOn = scheduledStartsOn.AddDays(30);

        Assert.NotNull(state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            selectedProgramme.ProgrammeId,
            selectedProgramme.RewardItemLabel,
            selectedProgramme.RewardThreshold,
            selectedProgramme.RewardCopy,
            scheduledStartsOn,
            scheduledEndsOn,
            BaseUri));

        var scheduledCard = state.GetWalletCard(joined!.CardId);
        Assert.NotNull(scheduledCard);
        Assert.Equal(CustomerCardStatus.Scheduled, scheduledCard!.CustomerCardStatus);
        Assert.Equal("Scheduled", scheduledCard.CustomerCardStatusLabel);
        Assert.False(scheduledCard.CanRedeem);

        var expiredStartsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-40);
        var expiredEndsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-2);

        Assert.NotNull(state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            selectedProgramme.ProgrammeId,
            selectedProgramme.RewardItemLabel,
            selectedProgramme.RewardThreshold,
            selectedProgramme.RewardCopy,
            expiredStartsOn,
            expiredEndsOn,
            BaseUri));

        var expiredCard = state.GetWalletCard(joined.CardId);
        Assert.NotNull(expiredCard);
        Assert.Equal(CustomerCardStatus.Expired, expiredCard!.CustomerCardStatus);
        Assert.Equal("Expired", expiredCard.CustomerCardStatusLabel);
        Assert.False(expiredCard.CanRedeem);
    }

    [Fact]
    public async Task RemoveWalletPushTokens_RemovesOnlyMatchingTokensForSelectedCard()
    {
        var state = CreateState();
        var workspace = await CreateMerchantWithProgrammeAsync(state);
        var selectedProgramme = Assert.IsType<LoyaltyProgrammeSnapshot>(workspace.SelectedProgramme);

        var cardOne = state.JoinCustomer(selectedProgramme.JoinCode);
        var cardTwo = state.JoinCustomer(selectedProgramme.JoinCode);

        Assert.NotNull(cardOne);
        Assert.NotNull(cardTwo);

        var deliveryOne = state.GetWalletPassDelivery(cardOne!.CardId);
        var deliveryTwo = state.GetWalletPassDelivery(cardTwo!.CardId);

        Assert.NotNull(deliveryOne);
        Assert.NotNull(deliveryTwo);

        Assert.Equal(
            WalletPassRegistrationStatus.Created,
            state.RegisterWalletPassDevice("device-card-one-1", cardOne.WalletPassId, deliveryOne!.AuthenticationToken, "push-token-invalid").Status);
        Assert.Equal(
            WalletPassRegistrationStatus.Created,
            state.RegisterWalletPassDevice("device-card-one-2", cardOne.WalletPassId, deliveryOne.AuthenticationToken, "push-token-keep").Status);
        Assert.Equal(
            WalletPassRegistrationStatus.Created,
            state.RegisterWalletPassDevice("device-card-two-1", cardTwo.WalletPassId, deliveryTwo!.AuthenticationToken, "push-token-invalid").Status);

        var removed = state.RemoveWalletPushTokens(cardOne.CardId, ["push-token-invalid"]);

        Assert.Equal(1, removed);
        Assert.Equal(["push-token-keep"], state.GetWalletPushTokens(cardOne.CardId));
        Assert.Equal(["push-token-invalid"], state.GetWalletPushTokens(cardTwo.CardId));
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
    public async Task ConfigureMerchantAccess_ReturnsAlreadyConfigured_WhenCredentialsExist()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);

        var first = state.ConfigureMerchantAccess(workspace.Merchant.MerchantId, "very-secure-password");
        var second = state.ConfigureMerchantAccess(workspace.Merchant.MerchantId, "another-secure-password");

        Assert.Equal(MerchantCredentialConfigurationStatus.Updated, first.Status);
        Assert.Equal(MerchantCredentialConfigurationStatus.AlreadyConfigured, second.Status);
    }

    [Fact]
    public async Task SetupChecklist_ShowsOwnerAccessConfigured_AfterCredentialSetup()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);
        Assert.False(workspace.SetupChecklist.OwnerAccessConfigured);

        Assert.Equal(
            MerchantCredentialConfigurationStatus.Updated,
            state.ConfigureMerchantAccess(workspace.Merchant.MerchantId, "very-secure-password").Status);

        var refreshed = state.GetMerchantWorkspace(workspace.Merchant.MerchantId, BaseUri);

        Assert.NotNull(refreshed);
        Assert.True(refreshed!.SetupChecklist.OwnerAccessConfigured);
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

    private static DemoPlatformState CreateState(ProgrammeCatalogueSnapshot? programmeCatalogue = null) =>
        new(
            NullLogger<DemoPlatformState>.Instance,
            new InMemoryMerchantBrandAssetStore(),
            programmeCatalogue is null ? null : new InMemoryProgrammeCatalogueRepository(programmeCatalogue));

    private static ProgrammeCatalogueSnapshot CreateVisibilityManagedCatalogue() =>
        new(
            [
                new ShopTypeOption(
                    "coffee",
                    "Coffee shop",
                    "Coffee-first operators.",
                    true),
                new ShopTypeOption(
                    "barber",
                    "Barbershop",
                    "Service-led grooming operators.",
                    false)
            ],
            [
                new CardTypeOption(
                    "wallet-loyalty-card",
                    "Wallet loyalty card",
                    "Repeat-visit digital loyalty card.",
                    true),
                new CardTypeOption(
                    "wallet-membership-card",
                    "Wallet membership card",
                    "Subscription and VIP card.",
                    false)
            ],
            [
                CreateTemplate("visible-template", "coffee", "wallet-loyalty-card", true),
                CreateTemplate("inactive-template", "coffee", "wallet-loyalty-card", false),
                CreateTemplate("inactive-shop-template", "barber", "wallet-loyalty-card", true),
                CreateTemplate("inactive-card-template", "coffee", "wallet-membership-card", true)
            ]);

    private static ProgrammeTemplateOption CreateTemplate(
        string templateKey,
        string shopTypeKey,
        string cardTypeKey,
        bool isActive) =>
        new(
            templateKey,
            "Template label",
            "visit-reward",
            "Visit reward",
            "Template headline",
            "Template description",
            "visits",
            5,
            "Reward copy",
            "apple-wallet-pass",
            "Apple Wallet pass",
            "Wallet loyalty card",
            shopTypeKey,
            cardTypeKey,
            cardTypeKey == "wallet-membership-card" ? "Wallet membership card" : "Wallet loyalty card",
            isActive);

    private static Task<MerchantWorkspaceSnapshot> CreateMerchantAsync(
        DemoPlatformState state,
        string shopTypeKey = "coffee") =>
        state.CreateMerchantAsync(
            "Jo's Coffee",
            "Bristol",
            "BS1 4DJ",
            "owner@joscoffee.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: DefaultPrimaryColor,
            accentColor: DefaultAccentColor,
            baseUri: BaseUri,
            shopTypeKey: shopTypeKey);

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

    private sealed class InMemoryProgrammeCatalogueRepository(ProgrammeCatalogueSnapshot snapshot)
        : IProgrammeCatalogueRepository
    {
        public ProgrammeCatalogueSnapshot Load() => snapshot;
    }
}
