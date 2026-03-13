using System.Collections.Concurrent;
using Fic.Contracts;
using Fic.LoyaltyCore;
using Fic.MerchantAccounts;
using Fic.WalletPasses;

namespace Fic.Platform.Web.Services;

public sealed class DemoPlatformState(
    ILogger<DemoPlatformState> logger,
    IMerchantBrandAssetStore brandAssetStore)
{
    private readonly Lock _gate = new();
    private readonly ConcurrentDictionary<Guid, MerchantAccount> _merchants = new();
    private readonly ConcurrentDictionary<Guid, BrandProfile> _brands = new();
    private readonly ConcurrentDictionary<Guid, LoyaltyProgramme> _programmes = new();
    private readonly ConcurrentDictionary<string, Guid> _joinCodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, CustomerCard> _cards = new();
    private readonly ConcurrentDictionary<string, Guid> _cardCodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, CustomerProgress> _progress = new();
    private readonly List<TimelineEventSnapshot> _timeline = [];

    public event Action? Changed;

    public IReadOnlyList<MerchantSummarySnapshot> GetMerchantSummaries()
    {
        lock (_gate)
        {
            return _merchants.Values
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(merchant =>
                {
                    var programme = _programmes.Values.Single(x => x.MerchantId == merchant.MerchantId);
                    var brand = _brands[merchant.MerchantId];
                    var cards = _cards.Values.Where(x => x.MerchantId == merchant.MerchantId).ToArray();
                    var unlocked = cards.Count(card => _progress[card.CardId].RewardState == RewardState.Unlocked);
                    return new MerchantSummarySnapshot(
                        merchant.MerchantId,
                        merchant.DisplayName,
                        $"Buy {programme.RewardThreshold} {programme.RewardItemLabel}, get 1 free",
                        cards.Length,
                        unlocked,
                        programme.JoinCode,
                        brand.LogoUrl,
                        brand.PrimaryColor,
                        brand.AccentColor,
                        brand.LogoWidth,
                        brand.LogoHeight);
                })
                .ToArray();
        }
    }

    public async Task<MerchantWorkspaceSnapshot> CreateMerchantAsync(
        string displayName,
        string contactEmail,
        string rewardItemLabel,
        int rewardThreshold,
        string rewardCopy,
        MerchantLogoUpload? logoUpload,
        string fallbackLogoUrl,
        string primaryColor,
        string accentColor,
        string baseUri,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var merchant = new MerchantAccount(Guid.NewGuid(), displayName.Trim(), contactEmail.Trim(), now);
        var logoMetadata = logoUpload is null
            ? new MerchantLogoMetadata(160, 160)
            : MerchantBrandAssetInspector.ReadLogoMetadata(logoUpload.Bytes);
        var logoUrl = logoUpload is null
            ? fallbackLogoUrl.Trim()
            : await brandAssetStore.SaveLogoAsync(merchant.MerchantId, logoUpload, cancellationToken);
        var brand = new BrandProfile(
            logoUrl,
            NormalizeColor(primaryColor),
            NormalizeColor(accentColor),
            logoMetadata.Width,
            logoMetadata.Height);
        var joinCode = $"join-{Guid.NewGuid():N}"[..13];
        var programme = new LoyaltyProgramme(
            Guid.NewGuid(),
            merchant.MerchantId,
            rewardItemLabel.Trim(),
            rewardThreshold,
            rewardCopy.Trim(),
            joinCode,
            now);

        lock (_gate)
        {
            _merchants[merchant.MerchantId] = merchant;
            _brands[merchant.MerchantId] = brand;
            _programmes[programme.ProgrammeId] = programme;
            _joinCodes[joinCode] = programme.ProgrammeId;

            AppendEvent(new MerchantAccountCreated(
                merchant.MerchantId,
                merchant.DisplayName,
                merchant.ContactEmail,
                now),
                $"{merchant.DisplayName} onboarded for the internal demo.");

            AppendEvent(new ProgrammeConfigured(
                merchant.MerchantId,
                programme.ProgrammeId,
                programme.RewardItemLabel,
                programme.RewardThreshold,
                now),
                $"Configured loyalty card: buy {programme.RewardThreshold} {programme.RewardItemLabel}, get 1 free.");

            logger.LogInformation(
                "merchant_created merchant_id={MerchantId} programme_id={ProgrammeId} threshold={RewardThreshold} logo_source={LogoSource}",
                merchant.MerchantId,
                programme.ProgrammeId,
                programme.RewardThreshold,
                logoUpload is null ? "fallback" : "stored");

            return BuildWorkspaceSnapshot(merchant.MerchantId, baseUri);
        }
    }

    public MerchantWorkspaceSnapshot? GetMerchantWorkspace(Guid merchantId, string baseUri)
    {
        lock (_gate)
        {
            return !_merchants.ContainsKey(merchantId) ? null : BuildWorkspaceSnapshot(merchantId, baseUri);
        }
    }

    public JoinExperienceSnapshot? GetJoinExperience(string joinCode)
    {
        lock (_gate)
        {
            if (!_joinCodes.TryGetValue(joinCode, out var programmeId) || !_programmes.TryGetValue(programmeId, out var programme))
            {
                return null;
            }

            var merchant = _merchants[programme.MerchantId];
            var brand = _brands[programme.MerchantId];

            return new JoinExperienceSnapshot(
                ToMerchantSnapshot(merchant),
                ToBrandSnapshot(brand),
                ToProgrammeSnapshot(programme));
        }
    }

    public WalletCardSnapshot? JoinCustomer(string joinCode)
    {
        lock (_gate)
        {
            if (!_joinCodes.TryGetValue(joinCode, out var programmeId) || !_programmes.TryGetValue(programmeId, out var programme))
            {
                return null;
            }

            var now = DateTimeOffset.UtcNow;
            var card = new CustomerCard(
                Guid.NewGuid(),
                programme.MerchantId,
                programme.ProgrammeId,
                $"card-{Guid.NewGuid():N}"[..14],
                $"wallet-{Guid.NewGuid():N}"[..16],
                now);

            var progress = new CustomerProgress(
                card.CardId,
                0,
                programme.RewardThreshold,
                BuildProgressText(0, programme.RewardThreshold, programme.RewardItemLabel),
                RewardState.Locked,
                now);

            _cards[card.CardId] = card;
            _cardCodes[card.CardCode] = card.CardId;
            _progress[card.CardId] = progress;

            AppendEvent(new CustomerJoined(
                programme.MerchantId,
                programme.ProgrammeId,
                card.CardId,
                card.CardCode,
                now),
                $"Customer joined { _merchants[programme.MerchantId].DisplayName } via QR and received card {card.CardCode}.");

            logger.LogInformation(
                "customer_joined merchant_id={MerchantId} programme_id={ProgrammeId} card_id={CardId} wallet_pass_id={WalletPassId}",
                programme.MerchantId,
                programme.ProgrammeId,
                card.CardId,
                card.WalletPassId);

            return BuildWalletCardSnapshot(card.CardId);
        }
    }

    public WalletCardSnapshot? GetWalletCard(Guid cardId)
    {
        lock (_gate)
        {
            return _cards.ContainsKey(cardId) ? BuildWalletCardSnapshot(cardId) : null;
        }
    }

    public async Task<MerchantWorkspaceSnapshot?> UpdateMerchantBrandAsync(
        Guid merchantId,
        string displayName,
        string primaryColor,
        string accentColor,
        MerchantLogoUpload? logoUpload,
        string baseUri,
        CancellationToken cancellationToken = default)
    {
        MerchantAccount merchant;
        BrandProfile brand;

        lock (_gate)
        {
            if (!_merchants.TryGetValue(merchantId, out merchant!) || !_brands.TryGetValue(merchantId, out brand!))
            {
                return null;
            }
        }

        var nextLogoUrl = brand.LogoUrl;
        var nextLogoWidth = brand.LogoWidth;
        var nextLogoHeight = brand.LogoHeight;

        if (logoUpload is not null)
        {
            var logoMetadata = MerchantBrandAssetInspector.ReadLogoMetadata(logoUpload.Bytes);
            nextLogoUrl = await brandAssetStore.SaveLogoAsync(merchantId, logoUpload, cancellationToken);
            nextLogoWidth = logoMetadata.Width;
            nextLogoHeight = logoMetadata.Height;
        }

        var updatedMerchant = merchant with
        {
            DisplayName = displayName.Trim()
        };
        var updatedBrand = brand with
        {
            LogoUrl = nextLogoUrl,
            PrimaryColor = NormalizeColor(primaryColor),
            AccentColor = NormalizeColor(accentColor),
            LogoWidth = nextLogoWidth,
            LogoHeight = nextLogoHeight
        };

        lock (_gate)
        {
            _merchants[merchantId] = updatedMerchant;
            _brands[merchantId] = updatedBrand;

            AppendEvent(new MerchantBrandUpdated(
                merchantId,
                updatedMerchant.DisplayName,
                DateTimeOffset.UtcNow),
                $"Updated workspace branding for {updatedMerchant.DisplayName}.");

            logger.LogInformation(
                "merchant_brand_updated merchant_id={MerchantId} display_name={DisplayName} logo_updated={LogoUpdated}",
                merchantId,
                updatedMerchant.DisplayName,
                logoUpload is not null);

            return BuildWorkspaceSnapshot(merchantId, baseUri);
        }
    }

    public MerchantWorkspaceSnapshot? UpdateProgramme(
        Guid merchantId,
        string rewardItemLabel,
        int rewardThreshold,
        string rewardCopy,
        string baseUri)
    {
        lock (_gate)
        {
            var programme = _programmes.Values.SingleOrDefault(x => x.MerchantId == merchantId);
            if (programme is null)
            {
                return null;
            }

            var updatedProgramme = programme with
            {
                RewardItemLabel = rewardItemLabel.Trim(),
                RewardThreshold = rewardThreshold,
                RewardCopy = rewardCopy.Trim(),
                ConfiguredAtUtc = DateTimeOffset.UtcNow
            };

            _programmes[programme.ProgrammeId] = updatedProgramme;

            foreach (var card in _cards.Values.Where(x => x.ProgrammeId == programme.ProgrammeId))
            {
                var current = _progress[card.CardId];
                var adjustedCount = Math.Min(current.CurrentCount, rewardThreshold);
                var adjustedState = current.RewardState switch
                {
                    RewardState.Redeemed => RewardState.Redeemed,
                    _ when adjustedCount >= rewardThreshold => RewardState.Unlocked,
                    _ => RewardState.Locked
                };

                _progress[card.CardId] = current with
                {
                    CurrentCount = adjustedState == RewardState.Redeemed ? 0 : adjustedCount,
                    TargetCount = rewardThreshold,
                    RewardState = adjustedState,
                    ProgressDisplayText = BuildProgressText(
                        adjustedState == RewardState.Redeemed ? 0 : adjustedCount,
                        rewardThreshold,
                        updatedProgramme.RewardItemLabel),
                    LastUpdatedUtc = DateTimeOffset.UtcNow
                };
            }

            AppendEvent(new CardTemplateUpdated(
                merchantId,
                programme.ProgrammeId,
                updatedProgramme.RewardItemLabel,
                updatedProgramme.RewardThreshold,
                updatedProgramme.ConfiguredAtUtc),
                $"Updated loyalty card template: buy {updatedProgramme.RewardThreshold} {updatedProgramme.RewardItemLabel}, get 1 free.");

            logger.LogInformation(
                "card_template_updated merchant_id={MerchantId} programme_id={ProgrammeId} threshold={RewardThreshold}",
                merchantId,
                programme.ProgrammeId,
                updatedProgramme.RewardThreshold);

            return BuildWorkspaceSnapshot(merchantId, baseUri);
        }
    }

    public MerchantWorkspaceSnapshot? AwardVisit(Guid merchantId, string scannedCode, string baseUri)
    {
        lock (_gate)
        {
            if (!_cardCodes.TryGetValue(scannedCode.Trim(), out var cardId) || !_cards.TryGetValue(cardId, out var card) || card.MerchantId != merchantId)
            {
                logger.LogWarning(
                    "visit_award_rejected merchant_id={MerchantId} scanned_code={ScannedCode}",
                    merchantId,
                    scannedCode);
                return null;
            }

            var programme = _programmes[card.ProgrammeId];
            var current = _progress[card.CardId];
            var nextCount = Math.Min(current.CurrentCount + 1, current.TargetCount);
            var nextState = nextCount >= current.TargetCount ? RewardState.Unlocked : RewardState.Locked;
            var updated = current with
            {
                CurrentCount = nextCount,
                RewardState = nextState,
                ProgressDisplayText = BuildProgressText(nextCount, current.TargetCount, programme.RewardItemLabel),
                LastUpdatedUtc = DateTimeOffset.UtcNow
            };

            _progress[card.CardId] = updated;

            AppendEvent(new VisitAwarded(
                merchantId,
                card.CardId,
                updated.CurrentCount,
                updated.TargetCount,
                updated.LastUpdatedUtc),
                $"Visit awarded to {card.CardCode}. Progress is now {updated.ProgressDisplayText}.");

            logger.LogInformation(
                "visit_awarded merchant_id={MerchantId} card_id={CardId} current_count={CurrentCount} target_count={TargetCount}",
                merchantId,
                card.CardId,
                updated.CurrentCount,
                updated.TargetCount);

            return BuildWorkspaceSnapshot(merchantId, baseUri);
        }
    }

    public MerchantWorkspaceSnapshot? RedeemReward(Guid merchantId, Guid cardId, string baseUri)
    {
        lock (_gate)
        {
            if (!_cards.TryGetValue(cardId, out var card) || card.MerchantId != merchantId)
            {
                return null;
            }

            var current = _progress[card.CardId];
            if (current.RewardState != RewardState.Unlocked)
            {
                return BuildWorkspaceSnapshot(merchantId, baseUri);
            }

            var programme = _programmes[card.ProgrammeId];
            var updated = current with
            {
                CurrentCount = 0,
                RewardState = RewardState.Redeemed,
                ProgressDisplayText = BuildProgressText(0, current.TargetCount, programme.RewardItemLabel),
                LastUpdatedUtc = DateTimeOffset.UtcNow
            };

            _progress[card.CardId] = updated;

            AppendEvent(new RewardRedeemed(
                merchantId,
                cardId,
                updated.LastUpdatedUtc),
                $"Reward redeemed for {card.CardCode}. Card reset to {updated.ProgressDisplayText}.");

            logger.LogInformation(
                "reward_redeemed merchant_id={MerchantId} card_id={CardId}",
                merchantId,
                cardId);

            return BuildWorkspaceSnapshot(merchantId, baseUri);
        }
    }

    public WalletCardView? GetWalletCardView(Guid cardId)
    {
        lock (_gate)
        {
            var snapshot = BuildWalletCardSnapshot(cardId);
            return snapshot is null
                ? null
                : new WalletCardView(
                    snapshot.CardId,
                    snapshot.VendorDisplayName,
                    snapshot.RewardItemLabel,
                    snapshot.RewardCopy,
                    snapshot.PrimaryColor,
                    snapshot.AccentColor,
                    snapshot.ProgressDisplayText,
                    snapshot.RewardState,
                    snapshot.WalletPassId);
        }
    }

    private MerchantWorkspaceSnapshot BuildWorkspaceSnapshot(Guid merchantId, string baseUri)
    {
        var merchant = _merchants[merchantId];
        var brand = _brands[merchantId];
        var programme = _programmes.Values.Single(x => x.MerchantId == merchantId);
        var joinUrl = new Uri(new Uri(baseUri, UriKind.Absolute), $"join/{programme.JoinCode}").ToString();
        var cards = _cards.Values
            .Where(x => x.MerchantId == merchantId)
            .OrderByDescending(x => x.JoinedAtUtc)
            .Select(x => BuildWalletCardSnapshot(x.CardId)!)
            .ToArray();
        var timeline = _timeline
            .Where(x => cards.Any(card => x.Summary.Contains(card.CardCode, StringComparison.OrdinalIgnoreCase)) || x.Summary.Contains(merchant.DisplayName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(12)
            .ToArray();

        return new MerchantWorkspaceSnapshot(
            ToMerchantSnapshot(merchant),
            ToBrandSnapshot(brand),
            ToProgrammeSnapshot(programme),
            joinUrl,
            cards,
            timeline);
    }

    private WalletCardSnapshot? BuildWalletCardSnapshot(Guid cardId)
    {
        if (!_cards.TryGetValue(cardId, out var card) || !_progress.TryGetValue(cardId, out var progress))
        {
            return null;
        }

        var merchant = _merchants[card.MerchantId];
        var programme = _programmes[card.ProgrammeId];
        var brand = _brands[card.MerchantId];

        return new WalletCardSnapshot(
            card.CardId,
            card.MerchantId,
            card.ProgrammeId,
            card.CardCode,
            card.WalletPassId,
            merchant.DisplayName,
            brand.LogoUrl,
            programme.RewardItemLabel,
            programme.RewardCopy,
            brand.PrimaryColor,
            brand.AccentColor,
            brand.LogoWidth,
            brand.LogoHeight,
            progress.CurrentCount,
            progress.TargetCount,
            progress.ProgressDisplayText,
            progress.RewardState,
            progress.LastUpdatedUtc);
    }

    private static string BuildProgressText(int currentCount, int targetCount, string rewardItemLabel) =>
        $"{currentCount}/{targetCount} {rewardItemLabel.Trim().ToLowerInvariant()}";

    private static string NormalizeColor(string value)
    {
        var normalized = value.Trim();
        return normalized.StartsWith('#') ? normalized : $"#{normalized}";
    }

    private MerchantAccountSnapshot ToMerchantSnapshot(MerchantAccount merchant) =>
        new(merchant.MerchantId, merchant.DisplayName, merchant.ContactEmail, merchant.CreatedAtUtc);

    private BrandProfileSnapshot ToBrandSnapshot(BrandProfile brand) =>
        new(brand.LogoUrl, brand.PrimaryColor, brand.AccentColor, brand.LogoWidth, brand.LogoHeight);

    private LoyaltyProgrammeSnapshot ToProgrammeSnapshot(LoyaltyProgramme programme) =>
        new(programme.ProgrammeId, programme.RewardItemLabel, programme.RewardThreshold, programme.RewardCopy, programme.JoinCode);

    private void AppendEvent(Fic.BuildingBlocks.DomainEvent domainEvent, string summary)
    {
        _timeline.Add(new TimelineEventSnapshot(domainEvent.EventId, domainEvent.EventType, summary, domainEvent.OccurredAtUtc));
        Changed?.Invoke();
    }
}
