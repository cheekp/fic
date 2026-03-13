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
    private const string StarterRewardItemLabel = "coffees";
    private const int StarterRewardThreshold = 5;
    private const int StarterProgrammeDurationDays = 365;

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
                    var programme = _programmes.Values
                        .Where(x => x.MerchantId == merchant.MerchantId)
                        .OrderByDescending(x => x.ConfiguredAtUtc)
                        .First();
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
        string townOrCity,
        string postcode,
        string contactEmail,
        MerchantLogoUpload? logoUpload,
        string fallbackLogoUrl,
        string primaryColor,
        string accentColor,
        string baseUri,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var merchant = new MerchantAccount(
            Guid.NewGuid(),
            displayName.Trim(),
            townOrCity.Trim(),
            postcode.Trim(),
            contactEmail.Trim(),
            now);
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
        var programme = BuildStarterProgramme(merchant.MerchantId, now);

        lock (_gate)
        {
            _merchants[merchant.MerchantId] = merchant;
            _brands[merchant.MerchantId] = brand;
            _programmes[programme.ProgrammeId] = programme;
            _joinCodes[programme.JoinCode] = programme.ProgrammeId;

            AppendEvent(new MerchantAccountCreated(
                merchant.MerchantId,
                merchant.DisplayName,
                merchant.TownOrCity,
                merchant.Postcode,
                merchant.ContactEmail,
                now),
                $"{merchant.DisplayName} onboarded for the internal demo in {merchant.TownOrCity}.");

            AppendEvent(new ProgrammeConfigured(
                merchant.MerchantId,
                programme.ProgrammeId,
                programme.RewardItemLabel,
                programme.RewardThreshold,
                now),
                $"Created starter loyalty card: buy {programme.RewardThreshold} {programme.RewardItemLabel}, get 1 free.");

            logger.LogInformation(
                "merchant_created merchant_id={MerchantId} programme_id={ProgrammeId} threshold={RewardThreshold} logo_source={LogoSource}",
                merchant.MerchantId,
                programme.ProgrammeId,
                programme.RewardThreshold,
                logoUpload is null ? "fallback" : "stored");

            return BuildWorkspaceSnapshot(merchant.MerchantId, baseUri, programme.ProgrammeId);
        }
    }

    public MerchantWorkspaceSnapshot? GetMerchantWorkspace(Guid merchantId, string baseUri, Guid? selectedProgrammeId = null)
    {
        lock (_gate)
        {
            return !_merchants.ContainsKey(merchantId) ? null : BuildWorkspaceSnapshot(merchantId, baseUri, selectedProgrammeId);
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

            if (!IsProgrammeActiveOn(programme, GetTodayUtc()))
            {
                logger.LogWarning(
                    "customer_join_rejected programme_id={ProgrammeId} reason=inactive_window join_code={JoinCode}",
                    programme.ProgrammeId,
                    joinCode);
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
        string townOrCity,
        string postcode,
        string contactEmail,
        string primaryColor,
        string accentColor,
        MerchantLogoUpload? logoUpload,
        Guid? selectedProgrammeId,
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
            DisplayName = displayName.Trim(),
            TownOrCity = townOrCity.Trim(),
            Postcode = postcode.Trim(),
            ContactEmail = contactEmail.Trim()
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

            return BuildWorkspaceSnapshot(merchantId, baseUri, selectedProgrammeId);
        }
    }

    public MerchantWorkspaceSnapshot? CreateProgramme(Guid merchantId, string baseUri)
    {
        lock (_gate)
        {
            if (!_merchants.ContainsKey(merchantId))
            {
                return null;
            }

            var now = DateTimeOffset.UtcNow;
            var programme = BuildStarterProgramme(merchantId, now);

            _programmes[programme.ProgrammeId] = programme;
            _joinCodes[programme.JoinCode] = programme.ProgrammeId;

            AppendEvent(new ProgrammeConfigured(
                merchantId,
                programme.ProgrammeId,
                programme.RewardItemLabel,
                programme.RewardThreshold,
                now),
                $"Added loyalty card {programme.ProgrammeId.ToString()[..8]} with {programme.RewardThreshold} {programme.RewardItemLabel} before reward.");

            return BuildWorkspaceSnapshot(merchantId, baseUri, programme.ProgrammeId);
        }
    }

    public MerchantWorkspaceSnapshot? UpdateProgramme(
        Guid merchantId,
        Guid programmeId,
        string rewardItemLabel,
        int rewardThreshold,
        string rewardCopy,
        DateOnly startsOn,
        DateOnly endsOn,
        string baseUri)
    {
        lock (_gate)
        {
            if (!_programmes.TryGetValue(programmeId, out var programme) || programme.MerchantId != merchantId)
            {
                return null;
            }

            if (endsOn < startsOn)
            {
                throw new InvalidOperationException("Expiry date must be on or after the begin date.");
            }

            var updatedProgramme = programme with
            {
                RewardItemLabel = rewardItemLabel.Trim(),
                RewardThreshold = rewardThreshold,
                RewardCopy = rewardCopy.Trim(),
                StartsOn = startsOn,
                EndsOn = endsOn,
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

            return BuildWorkspaceSnapshot(merchantId, baseUri, programmeId);
        }
    }

    public MerchantWorkspaceSnapshot? AwardVisit(Guid merchantId, Guid programmeId, string scannedCode, string baseUri)
    {
        lock (_gate)
        {
            if (!_cardCodes.TryGetValue(scannedCode.Trim(), out var cardId)
                || !_cards.TryGetValue(cardId, out var card)
                || card.MerchantId != merchantId
                || card.ProgrammeId != programmeId)
            {
                logger.LogWarning(
                    "visit_award_rejected merchant_id={MerchantId} programme_id={ProgrammeId} scanned_code={ScannedCode}",
                    merchantId,
                    programmeId,
                    scannedCode);
                return null;
            }

            var programme = _programmes[card.ProgrammeId];
            if (!IsProgrammeActiveOn(programme, GetTodayUtc()))
            {
                logger.LogWarning(
                    "visit_award_rejected merchant_id={MerchantId} programme_id={ProgrammeId} reason=inactive_window scanned_code={ScannedCode}",
                    merchantId,
                    programmeId,
                    scannedCode);
                return null;
            }

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

            return BuildWorkspaceSnapshot(merchantId, baseUri, programmeId);
        }
    }

    public MerchantWorkspaceSnapshot? RedeemReward(Guid merchantId, Guid programmeId, Guid cardId, string baseUri)
    {
        lock (_gate)
        {
            if (!_cards.TryGetValue(cardId, out var card) || card.MerchantId != merchantId || card.ProgrammeId != programmeId)
            {
                return null;
            }

            var current = _progress[card.CardId];
            if (current.RewardState != RewardState.Unlocked)
            {
                return BuildWorkspaceSnapshot(merchantId, baseUri, programmeId);
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

            return BuildWorkspaceSnapshot(merchantId, baseUri, programmeId);
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

    private MerchantWorkspaceSnapshot BuildWorkspaceSnapshot(Guid merchantId, string baseUri, Guid? selectedProgrammeId = null)
    {
        var merchant = _merchants[merchantId];
        var brand = _brands[merchantId];
        var merchantProgrammes = _programmes.Values
            .Where(x => x.MerchantId == merchantId)
            .OrderByDescending(x => x.ConfiguredAtUtc)
            .ToArray();
        var selectedProgramme = merchantProgrammes.FirstOrDefault(x => x.ProgrammeId == selectedProgrammeId) ?? merchantProgrammes.First();
        var selectedJoinUrl = new Uri(new Uri(baseUri, UriKind.Absolute), $"join/{selectedProgramme.JoinCode}").ToString();
        var selectedCards = _cards.Values
            .Where(x => x.ProgrammeId == selectedProgramme.ProgrammeId)
            .OrderByDescending(x => x.JoinedAtUtc)
            .Select(x => BuildWalletCardSnapshot(x.CardId)!)
            .ToArray();
        var allCards = _cards.Values
            .Where(x => x.MerchantId == merchantId)
            .Select(x => BuildWalletCardSnapshot(x.CardId)!)
            .ToArray();
        var programmeSummaries = merchantProgrammes
            .Select(programme =>
            {
                var programmeCards = allCards.Where(card => card.ProgrammeId == programme.ProgrammeId).ToArray();
                return new ProgrammeSummarySnapshot(
                    programme.ProgrammeId,
                    BuildRewardHeadline(programme.RewardThreshold, programme.RewardItemLabel),
                    programme.RewardItemLabel,
                    programme.RewardThreshold,
                    programme.StartsOn,
                    programme.EndsOn,
                    programme.JoinCode,
                    programmeCards.Length,
                    programmeCards.Count(card => card.RewardState == RewardState.Unlocked));
            })
            .ToArray();
        var timeline = _timeline
            .Where(x =>
                selectedCards.Any(card => x.Summary.Contains(card.CardCode, StringComparison.OrdinalIgnoreCase))
                || x.Summary.Contains(merchant.DisplayName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(12)
            .ToArray();

        return new MerchantWorkspaceSnapshot(
            ToMerchantSnapshot(merchant),
            ToBrandSnapshot(brand),
            BuildSetupChecklist(merchant, brand, programmeSummaries),
            new MerchantInsightsSnapshot(
                programmeSummaries.Length,
                allCards.Length,
                allCards.Count(card => card.RewardState == RewardState.Unlocked)),
            programmeSummaries,
            ToProgrammeSnapshot(selectedProgramme),
            selectedJoinUrl,
            selectedCards,
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
            programme.StartsOn,
            programme.EndsOn,
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
        new(merchant.MerchantId, merchant.DisplayName, merchant.TownOrCity, merchant.Postcode, merchant.ContactEmail, merchant.CreatedAtUtc);

    private BrandProfileSnapshot ToBrandSnapshot(BrandProfile brand) =>
        new(brand.LogoUrl, brand.PrimaryColor, brand.AccentColor, brand.LogoWidth, brand.LogoHeight);

    private LoyaltyProgrammeSnapshot ToProgrammeSnapshot(LoyaltyProgramme programme) =>
        new(programme.ProgrammeId, programme.RewardItemLabel, programme.RewardThreshold, programme.RewardCopy, programme.StartsOn, programme.EndsOn, programme.JoinCode);

    private MerchantSetupChecklistSnapshot BuildSetupChecklist(
        MerchantAccount merchant,
        BrandProfile brand,
        IReadOnlyList<ProgrammeSummarySnapshot> programmeSummaries) =>
        new(
            !string.IsNullOrWhiteSpace(merchant.DisplayName)
            && !string.IsNullOrWhiteSpace(merchant.TownOrCity)
            && !string.IsNullOrWhiteSpace(merchant.Postcode)
            && !string.IsNullOrWhiteSpace(merchant.ContactEmail),
            !string.IsNullOrWhiteSpace(brand.LogoUrl)
            && !string.IsNullOrWhiteSpace(brand.PrimaryColor)
            && !string.IsNullOrWhiteSpace(brand.AccentColor),
            programmeSummaries.Count > 0,
            programmeSummaries.Any(summary => !string.IsNullOrWhiteSpace(summary.JoinCode)));

    private static string BuildRewardCopy(int rewardThreshold, string rewardItemLabel) =>
        $"Buy {rewardThreshold} {rewardItemLabel}, get one free.";

    private static string BuildRewardHeadline(int rewardThreshold, string rewardItemLabel) =>
        $"Buy {rewardThreshold} {rewardItemLabel}, get one free";

    private LoyaltyProgramme BuildStarterProgramme(Guid merchantId, DateTimeOffset configuredAtUtc)
    {
        var startsOn = DateOnly.FromDateTime(configuredAtUtc.UtcDateTime.Date);
        var endsOn = startsOn.AddDays(StarterProgrammeDurationDays);

        return new LoyaltyProgramme(
            Guid.NewGuid(),
            merchantId,
            StarterRewardItemLabel,
            StarterRewardThreshold,
            BuildRewardCopy(StarterRewardThreshold, StarterRewardItemLabel),
            startsOn,
            endsOn,
            $"join-{Guid.NewGuid():N}"[..13],
            configuredAtUtc);
    }

    private static DateOnly GetTodayUtc() => DateOnly.FromDateTime(DateTime.UtcNow);

    private static bool IsProgrammeActiveOn(LoyaltyProgramme programme, DateOnly onDate) =>
        onDate >= programme.StartsOn && onDate <= programme.EndsOn;

    private void AppendEvent(Fic.BuildingBlocks.DomainEvent domainEvent, string summary)
    {
        _timeline.Add(new TimelineEventSnapshot(domainEvent.EventId, domainEvent.EventType, summary, domainEvent.OccurredAtUtc));
        Changed?.Invoke();
    }
}
