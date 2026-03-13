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
    public async Task CreateMerchantAsync_CreatesStarterProgrammeWithOneYearWindow()
    {
        var state = CreateState();

        var workspace = await CreateMerchantAsync(state);

        Assert.Single(workspace.Programmes);
        Assert.Equal(workspace.SelectedProgramme.StartsOn.AddDays(365), workspace.SelectedProgramme.EndsOn);
        Assert.True(workspace.SetupChecklist.HasAnyProgramme);
        Assert.Equal(workspace.SelectedProgramme.ProgrammeId, workspace.Programmes[0].ProgrammeId);
    }

    [Fact]
    public async Task CreateProgramme_AddsSecondProgrammeAndSelectsIt()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);

        var updated = state.CreateProgramme(workspace.Merchant.MerchantId, BaseUri);

        Assert.NotNull(updated);
        Assert.Equal(2, updated!.Programmes.Count);
        Assert.NotEqual(workspace.SelectedProgramme.ProgrammeId, updated.SelectedProgramme.ProgrammeId);
        Assert.Contains(updated.SelectedProgramme.JoinCode, updated.SelectedJoinUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateProgramme_RejectsExpiryBeforeBegin()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);
        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(10);
        var endsOn = startsOn.AddDays(-1);

        var error = Assert.Throws<InvalidOperationException>(() =>
            state.UpdateProgramme(
                workspace.Merchant.MerchantId,
                workspace.SelectedProgramme.ProgrammeId,
                workspace.SelectedProgramme.RewardItemLabel,
                workspace.SelectedProgramme.RewardThreshold,
                workspace.SelectedProgramme.RewardCopy,
                startsOn,
                endsOn,
                BaseUri));

        Assert.Equal("Expiry date must be on or after the begin date.", error.Message);
    }

    [Fact]
    public async Task JoinCustomer_ReturnsNull_WhenProgrammeHasNotStarted()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);
        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(7);
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

        var joined = state.JoinCustomer(updated!.SelectedProgramme.JoinCode);

        Assert.Null(joined);
    }

    [Fact]
    public async Task AwardVisit_ReturnsNull_WhenProgrammeHasExpired()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);
        var joined = state.JoinCustomer(workspace.SelectedProgramme.JoinCode);
        Assert.NotNull(joined);

        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-30);
        var endsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-1);
        var expired = state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            workspace.SelectedProgramme.ProgrammeId,
            workspace.SelectedProgramme.RewardItemLabel,
            workspace.SelectedProgramme.RewardThreshold,
            workspace.SelectedProgramme.RewardCopy,
            startsOn,
            endsOn,
            BaseUri);

        var result = state.AwardVisit(
            workspace.Merchant.MerchantId,
            expired!.SelectedProgramme.ProgrammeId,
            joined!.CardCode,
            BaseUri);

        Assert.Null(result);
    }

    [Fact]
    public async Task WalletCardSnapshot_TracksUpdatedProgrammeDates()
    {
        var state = CreateState();
        var workspace = await CreateMerchantAsync(state);
        var joined = state.JoinCustomer(workspace.SelectedProgramme.JoinCode);
        Assert.NotNull(joined);

        var startsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-2);
        var endsOn = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(14);

        state.UpdateProgramme(
            workspace.Merchant.MerchantId,
            workspace.SelectedProgramme.ProgrammeId,
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
