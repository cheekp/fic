using System.Collections.Concurrent;
using Fic.Contracts;
using Fic.LoyaltyCore;
using Fic.MerchantAccounts;
using Fic.WalletPasses;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fic.Platform.Web.Services;

public sealed class DemoPlatformState(
    ILogger<DemoPlatformState> logger,
    IMerchantBrandAssetStore brandAssetStore,
    IProgrammeCatalogueRepository? programmeCatalogueRepository = null)
{
    private const int DefaultProgrammeDurationDays = 365;
    private readonly ProgrammeCatalogueSnapshot _programmeCatalogue =
        (programmeCatalogueRepository
         ?? new JsonProgrammeCatalogueRepository(NullLogger<JsonProgrammeCatalogueRepository>.Instance))
        .Load();

    private readonly Lock _gate = new();
    private readonly ConcurrentDictionary<Guid, MerchantAccount> _merchants = new();
    private readonly ConcurrentDictionary<Guid, BrandProfile> _brands = new();
    private readonly ConcurrentDictionary<Guid, LoyaltyProgramme> _programmes = new();
    private readonly ConcurrentDictionary<string, Guid> _joinCodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, CustomerCard> _cards = new();
    private readonly ConcurrentDictionary<Guid, CustomerCardLifecycleState> _cardLifecycle = new();
    private readonly ConcurrentDictionary<string, Guid> _cardCodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Guid> _walletPassIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, CustomerProgress> _progress = new();
    private readonly ConcurrentDictionary<string, WalletPassDeviceRegistration> _walletRegistrations = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<TimelineEventSnapshot> _timeline = [];

    public event Action? Changed;

    public IReadOnlyList<ShopTypeOption> GetShopTypes() =>
        _programmeCatalogue.ShopTypes
            .Where(type => type.IsActive)
            .ToArray();

    public IReadOnlyList<CardTypeOption> GetCardTypes() =>
        _programmeCatalogue.CardTypes
            .Where(type => type.IsActive)
            .ToArray();

    public IReadOnlyList<ProgrammeTemplateOption> GetProgrammeTemplates(string? shopTypeKey = null, string? cardTypeKey = null)
    {
        var normalizedShopTypeKey = NormalizeCatalogueFilterKey(shopTypeKey);
        var normalizedCardTypeKey = NormalizeCatalogueFilterKey(cardTypeKey);
        var activeShopTypes = GetShopTypes()
            .Select(type => type.ShopTypeKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var activeCardTypes = GetCardTypes()
            .Select(type => type.CardTypeKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return _programmeCatalogue.ProgrammeTemplates
            .Where(template =>
                template.IsActive
                && activeShopTypes.Contains(template.ShopTypeKey)
                && activeCardTypes.Contains(template.CardTypeKey)
                && (normalizedShopTypeKey is null
                    || string.Equals(template.ShopTypeKey, normalizedShopTypeKey, StringComparison.OrdinalIgnoreCase))
                && (normalizedCardTypeKey is null
                    || string.Equals(template.CardTypeKey, normalizedCardTypeKey, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
    }

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
                        .FirstOrDefault();
                    var brand = _brands[merchant.MerchantId];
                    var cards = _cards.Values.Where(x => x.MerchantId == merchant.MerchantId).ToArray();
                    var unlocked = cards.Count(card => _progress[card.CardId].RewardState == RewardState.Unlocked);
                    return new MerchantSummarySnapshot(
                        merchant.MerchantId,
                        merchant.DisplayName,
                        programme is null
                            ? "No programme yet"
                            : BuildRewardHeadline(programme.TemplateKey, programme.RewardThreshold, programme.RewardItemLabel),
                        cards.Length,
                        unlocked,
                        programme?.JoinCode ?? string.Empty,
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
        bool createStarterProgramme = false,
        string? shopTypeKey = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var resolvedShopTypeKey = ResolveShopTypeForCreate(shopTypeKey);
        var merchant = new MerchantAccount(
            Guid.NewGuid(),
            displayName.Trim(),
            townOrCity.Trim(),
            postcode.Trim(),
            contactEmail.Trim(),
            now,
            resolvedShopTypeKey);
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
        LoyaltyProgramme? starterProgramme = createStarterProgramme
            ? BuildProgrammeFromTemplate(merchant.MerchantId, GetProgrammeTemplate("coffee-visits")!, now)
            : null;

        lock (_gate)
        {
            _merchants[merchant.MerchantId] = merchant;
            _brands[merchant.MerchantId] = brand;

            AppendEvent(new MerchantAccountCreated(
                merchant.MerchantId,
                merchant.DisplayName,
                merchant.TownOrCity,
                merchant.Postcode,
                merchant.ContactEmail,
                merchant.ShopTypeKey,
                now),
                $"{merchant.DisplayName} onboarded for the internal demo in {merchant.TownOrCity}.");

            if (starterProgramme is not null)
            {
                _programmes[starterProgramme.ProgrammeId] = starterProgramme;
                _joinCodes[starterProgramme.JoinCode] = starterProgramme.ProgrammeId;

                AppendEvent(new ProgrammeConfigured(
                    merchant.MerchantId,
                    starterProgramme.ProgrammeId,
                    starterProgramme.RewardItemLabel,
                    starterProgramme.RewardThreshold,
                    now),
                    $"Created starter programme from {starterProgramme.TemplateLabel}: {starterProgramme.RewardCopy}");
            }

            logger.LogInformation(
                "merchant_created merchant_id={MerchantId} programme_id={ProgrammeId} logo_source={LogoSource} shop_type={ShopTypeKey}",
                merchant.MerchantId,
                starterProgramme?.ProgrammeId,
                logoUpload is null ? "fallback" : "stored",
                merchant.ShopTypeKey);

            return BuildWorkspaceSnapshot(merchant.MerchantId, baseUri, starterProgramme?.ProgrammeId);
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
                $"auth-{Guid.NewGuid():N}",
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
            _walletPassIds[card.WalletPassId] = card.CardId;
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

    public WalletPassDeliverySnapshot? GetWalletPassDelivery(Guid cardId)
    {
        lock (_gate)
        {
            if (!_cards.TryGetValue(cardId, out var card))
            {
                return null;
            }

            var snapshot = BuildWalletCardSnapshot(cardId);
            return snapshot is null ? null : new WalletPassDeliverySnapshot(snapshot, card.WalletAuthenticationToken);
        }
    }

    public WalletPassDeliverySnapshot? GetWalletPassDeliveryBySerialNumber(string serialNumber)
    {
        lock (_gate)
        {
            if (!_walletPassIds.TryGetValue(serialNumber, out var cardId) || !_cards.TryGetValue(cardId, out var card))
            {
                return null;
            }

            var snapshot = BuildWalletCardSnapshot(cardId);
            return snapshot is null ? null : new WalletPassDeliverySnapshot(snapshot, card.WalletAuthenticationToken);
        }
    }

    public WalletPassRegistrationResult RegisterWalletPassDevice(
        string deviceLibraryIdentifier,
        string serialNumber,
        string authenticationToken,
        string pushToken)
    {
        lock (_gate)
        {
            if (!_walletPassIds.TryGetValue(serialNumber, out var cardId) || !_cards.TryGetValue(cardId, out var card))
            {
                return new WalletPassRegistrationResult(WalletPassRegistrationStatus.NotFound);
            }

            if (!string.Equals(card.WalletAuthenticationToken, authenticationToken, StringComparison.Ordinal))
            {
                return new WalletPassRegistrationResult(WalletPassRegistrationStatus.Unauthorized);
            }

            var key = BuildWalletRegistrationKey(deviceLibraryIdentifier, serialNumber);
            var status = _walletRegistrations.ContainsKey(key)
                ? WalletPassRegistrationStatus.Updated
                : WalletPassRegistrationStatus.Created;

            _walletRegistrations[key] = new WalletPassDeviceRegistration(
                deviceLibraryIdentifier,
                serialNumber,
                cardId,
                pushToken.Trim(),
                DateTimeOffset.UtcNow);

            return new WalletPassRegistrationResult(status);
        }
    }

    public WalletPassRegistrationResult RemoveWalletPassDeviceRegistration(
        string deviceLibraryIdentifier,
        string serialNumber,
        string authenticationToken)
    {
        lock (_gate)
        {
            if (!_walletPassIds.TryGetValue(serialNumber, out var cardId) || !_cards.TryGetValue(cardId, out var card))
            {
                return new WalletPassRegistrationResult(WalletPassRegistrationStatus.NotFound);
            }

            if (!string.Equals(card.WalletAuthenticationToken, authenticationToken, StringComparison.Ordinal))
            {
                return new WalletPassRegistrationResult(WalletPassRegistrationStatus.Unauthorized);
            }

            var key = BuildWalletRegistrationKey(deviceLibraryIdentifier, serialNumber);
            _walletRegistrations.TryRemove(key, out _);
            return new WalletPassRegistrationResult(WalletPassRegistrationStatus.Updated);
        }
    }

    public WalletPassSerialUpdateSnapshot GetUpdatedWalletPassSerialNumbers(
        string deviceLibraryIdentifier,
        DateTimeOffset? passesUpdatedSince)
    {
        lock (_gate)
        {
            var matches = _walletRegistrations.Values
                .Where(registration => string.Equals(registration.DeviceLibraryIdentifier, deviceLibraryIdentifier, StringComparison.Ordinal))
                .Select(registration =>
                {
                    var card = BuildWalletCardSnapshot(registration.CardId);
                    return new
                    {
                        registration.SerialNumber,
                        Card = card
                    };
                })
                .Where(x => x.Card is not null)
                .ToArray();

            var changed = matches
                .Where(x => !passesUpdatedSince.HasValue || x.Card!.LastUpdatedUtc > passesUpdatedSince.Value)
                .OrderBy(x => x.Card!.LastUpdatedUtc)
                .ToArray();

            var lastUpdatedUtc = changed.Length > 0
                ? changed.Max(x => x.Card!.LastUpdatedUtc)
                : matches.Select(x => x.Card!.LastUpdatedUtc).DefaultIfEmpty().Max();

            var lastUpdatedTag = lastUpdatedUtc == default
                ? null
                : BuildWalletUpdateTag(lastUpdatedUtc);

            return new WalletPassSerialUpdateSnapshot(
                changed.Select(x => x.SerialNumber).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                lastUpdatedTag);
        }
    }

    public IReadOnlyList<string> GetWalletPushTokens(Guid cardId)
    {
        lock (_gate)
        {
            return _walletRegistrations.Values
                .Where(registration => registration.CardId == cardId)
                .Select(registration => registration.PushToken)
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
    }

    public int RemoveWalletPushTokens(Guid cardId, IReadOnlyList<string> pushTokens)
    {
        if (pushTokens.Count == 0)
        {
            return 0;
        }

        lock (_gate)
        {
            var candidateTokens = pushTokens
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Select(token => token.Trim())
                .ToHashSet(StringComparer.Ordinal);

            if (candidateTokens.Count == 0)
            {
                return 0;
            }

            var registrationsToRemove = _walletRegistrations
                .Where(entry =>
                    entry.Value.CardId == cardId
                    && candidateTokens.Contains(entry.Value.PushToken))
                .Select(entry => entry.Key)
                .ToArray();

            var removedCount = 0;
            foreach (var registrationKey in registrationsToRemove)
            {
                if (_walletRegistrations.TryRemove(registrationKey, out _))
                {
                    removedCount++;
                }
            }

            return removedCount;
        }
    }

    public MerchantAuthenticationResult AuthenticateMerchant(string email, string password)
    {
        lock (_gate)
        {
            var merchant = _merchants.Values
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefault(x => string.Equals(x.ContactEmail, email.Trim(), StringComparison.OrdinalIgnoreCase));

            if (merchant is null)
            {
                return new MerchantAuthenticationResult(MerchantAuthenticationStatus.NotFound);
            }

            if (string.IsNullOrWhiteSpace(merchant.PasswordHash) || string.IsNullOrWhiteSpace(merchant.PasswordSalt))
            {
                return new MerchantAuthenticationResult(MerchantAuthenticationStatus.CredentialsNotConfigured);
            }

            return MerchantPasswordHasher.VerifyPassword(password, merchant.PasswordHash, merchant.PasswordSalt)
                ? new MerchantAuthenticationResult(MerchantAuthenticationStatus.Authenticated, ToMerchantSnapshot(merchant))
                : new MerchantAuthenticationResult(MerchantAuthenticationStatus.InvalidCredentials);
        }
    }

    public MerchantCredentialConfigurationResult ConfigureMerchantAccess(Guid merchantId, string password)
    {
        lock (_gate)
        {
            if (!_merchants.TryGetValue(merchantId, out var merchant))
            {
                return new MerchantCredentialConfigurationResult(MerchantCredentialConfigurationStatus.NotFound);
            }

            if (!string.IsNullOrWhiteSpace(merchant.PasswordHash) && !string.IsNullOrWhiteSpace(merchant.PasswordSalt))
            {
                return new MerchantCredentialConfigurationResult(
                    MerchantCredentialConfigurationStatus.AlreadyConfigured,
                    ToMerchantSnapshot(merchant));
            }

            try
            {
                var (passwordHash, passwordSalt) = MerchantPasswordHasher.HashPassword(password);
                var updatedMerchant = merchant with
                {
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                };

                _merchants[merchantId] = updatedMerchant;
                return new MerchantCredentialConfigurationResult(
                    MerchantCredentialConfigurationStatus.Updated,
                    ToMerchantSnapshot(updatedMerchant));
            }
            catch (InvalidOperationException ex)
            {
                return new MerchantCredentialConfigurationResult(
                    MerchantCredentialConfigurationStatus.InvalidPassword,
                    ErrorMessage: ex.Message);
            }
        }
    }

    public async Task<MerchantWorkspaceSnapshot?> UpdateMerchantBrandAsync(
        Guid merchantId,
        string displayName,
        string townOrCity,
        string postcode,
        string contactEmail,
        string shopTypeKey,
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
            ContactEmail = contactEmail.Trim(),
            ShopTypeKey = ResolveShopTypeForUpdate(shopTypeKey, merchant.ShopTypeKey)
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

    public MerchantWorkspaceSnapshot? CreateProgramme(Guid merchantId, string templateKey, string baseUri)
    {
        lock (_gate)
        {
            if (!_merchants.ContainsKey(merchantId))
            {
                return null;
            }

            var merchant = _merchants[merchantId];
            var template = GetProgrammeTemplate(templateKey);
            if (template is null)
            {
                return null;
            }

            if (!string.Equals(template.ShopTypeKey, merchant.ShopTypeKey, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "programme_create_rejected merchant_id={MerchantId} template_key={TemplateKey} merchant_shop_type={MerchantShopType} template_shop_type={TemplateShopType}",
                    merchantId,
                    template.TemplateKey,
                    merchant.ShopTypeKey,
                    template.ShopTypeKey);
                return null;
            }

            var now = DateTimeOffset.UtcNow;
            var programme = BuildProgrammeFromTemplate(merchantId, template, now);

            _programmes[programme.ProgrammeId] = programme;
            _joinCodes[programme.JoinCode] = programme.ProgrammeId;

            AppendEvent(new ProgrammeConfigured(
                merchantId,
                programme.ProgrammeId,
                programme.RewardItemLabel,
                programme.RewardThreshold,
                now),
                $"Added programme from {programme.TemplateLabel}: {programme.RewardCopy}");

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

    public MerchantWorkspaceSnapshot? ApplyCardLifecycleAction(
        Guid merchantId,
        Guid programmeId,
        Guid cardId,
        string action,
        string baseUri)
    {
        lock (_gate)
        {
            if (!_cards.TryGetValue(cardId, out var card) || card.MerchantId != merchantId || card.ProgrammeId != programmeId)
            {
                return null;
            }

            if (!TryParseCardLifecycleAction(action, out var lifecycleState))
            {
                return null;
            }

            _cardLifecycle[cardId] = lifecycleState;
            var now = DateTimeOffset.UtcNow;
            if (_progress.TryGetValue(cardId, out var progress))
            {
                _progress[cardId] = progress with { LastUpdatedUtc = now };
            }

            AppendEvent(
                new CardLifecycleUpdated(merchantId, programmeId, cardId, lifecycleState.ToString(), now),
                $"Card {card.CardCode} set to {BuildLifecycleLabel(lifecycleState)}.");

            return BuildWorkspaceSnapshot(merchantId, baseUri, programmeId);
        }
    }

    public MerchantWorkspaceSnapshot? ApplyBulkCardLifecycleAction(
        Guid merchantId,
        Guid programmeId,
        IReadOnlyList<Guid> cardIds,
        string action,
        string baseUri)
    {
        lock (_gate)
        {
            if (!TryParseCardLifecycleAction(action, out var lifecycleState))
            {
                return null;
            }

            var validCards = cardIds
                .Distinct()
                .Where(cardId =>
                    _cards.TryGetValue(cardId, out var card)
                    && card.MerchantId == merchantId
                    && card.ProgrammeId == programmeId)
                .ToArray();

            if (validCards.Length == 0)
            {
                return BuildWorkspaceSnapshot(merchantId, baseUri, programmeId);
            }

            var now = DateTimeOffset.UtcNow;
            foreach (var cardId in validCards)
            {
                _cardLifecycle[cardId] = lifecycleState;
                if (_progress.TryGetValue(cardId, out var progress))
                {
                    _progress[cardId] = progress with { LastUpdatedUtc = now };
                }
            }

            AppendEvent(
                new CardLifecycleBulkUpdated(merchantId, programmeId, validCards.Length, lifecycleState.ToString(), now),
                $"Updated {validCards.Length} cards to {BuildLifecycleLabel(lifecycleState)}.");

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
        var selectedProgramme = merchantProgrammes.FirstOrDefault(x => x.ProgrammeId == selectedProgrammeId) ?? merchantProgrammes.FirstOrDefault();
        var selectedJoinUrl = selectedProgramme is null
            ? null
            : new Uri(new Uri(baseUri, UriKind.Absolute), $"join/{selectedProgramme.JoinCode}").ToString();
        var selectedCards = selectedProgramme is null
            ? []
            : _cards.Values
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
                    programme.TemplateKey,
                    programme.TemplateLabel,
                    programme.ProgrammeTypeKey,
                    programme.ProgrammeTypeLabel,
                    programme.DeliveryTypeKey,
                    programme.DeliveryTypeLabel,
                    programme.CardTypeKey,
                    programme.CardTypeLabel,
                    programme.OutputLabel,
                    BuildRewardHeadline(programme.TemplateKey, programme.RewardThreshold, programme.RewardItemLabel),
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
            selectedProgramme is null ? null : ToProgrammeSnapshot(selectedProgramme),
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
        var lifecycleState = _cardLifecycle.TryGetValue(card.CardId, out var state)
            ? state
            : CustomerCardLifecycleState.Active;
        var customerCardStatus = BuildCustomerCardStatus(programme, progress.RewardState, lifecycleState);

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
            customerCardStatus,
            BuildCustomerCardStatusLabel(customerCardStatus),
            customerCardStatus == CustomerCardStatus.RewardReady && lifecycleState == CustomerCardLifecycleState.Active,
            progress.LastUpdatedUtc);
    }

    private static string BuildProgressText(int currentCount, int targetCount, string rewardItemLabel) =>
        $"{currentCount}/{targetCount} {rewardItemLabel.Trim().ToLowerInvariant()}";

    private static string NormalizeColor(string value)
    {
        var normalized = value.Trim();
        return normalized.StartsWith('#') ? normalized : $"#{normalized}";
    }

    private static CustomerCardStatus BuildCustomerCardStatus(
        LoyaltyProgramme programme,
        RewardState rewardState,
        CustomerCardLifecycleState lifecycleState)
    {
        if (lifecycleState == CustomerCardLifecycleState.Suspended)
        {
            return CustomerCardStatus.Suspended;
        }

        if (lifecycleState == CustomerCardLifecycleState.Archived)
        {
            return CustomerCardStatus.Archived;
        }

        var today = GetTodayUtc();
        if (today < programme.StartsOn)
        {
            return CustomerCardStatus.Scheduled;
        }

        if (today > programme.EndsOn)
        {
            return CustomerCardStatus.Expired;
        }

        return rewardState switch
        {
            RewardState.Unlocked => CustomerCardStatus.RewardReady,
            RewardState.Redeemed => CustomerCardStatus.Redeemed,
            _ => CustomerCardStatus.Active
        };
    }

    private static string BuildCustomerCardStatusLabel(CustomerCardStatus status) => status switch
    {
        CustomerCardStatus.RewardReady => "Reward ready",
        CustomerCardStatus.Redeemed => "Redeemed",
        CustomerCardStatus.Scheduled => "Scheduled",
        CustomerCardStatus.Expired => "Expired",
        CustomerCardStatus.Suspended => "Suspended",
        CustomerCardStatus.Archived => "Archived",
        _ => "Active"
    };

    private static bool TryParseCardLifecycleAction(string action, out CustomerCardLifecycleState lifecycleState)
    {
        if (string.Equals(action, "suspend", StringComparison.OrdinalIgnoreCase))
        {
            lifecycleState = CustomerCardLifecycleState.Suspended;
            return true;
        }

        if (string.Equals(action, "archive", StringComparison.OrdinalIgnoreCase))
        {
            lifecycleState = CustomerCardLifecycleState.Archived;
            return true;
        }

        if (string.Equals(action, "reactivate", StringComparison.OrdinalIgnoreCase))
        {
            lifecycleState = CustomerCardLifecycleState.Active;
            return true;
        }

        lifecycleState = CustomerCardLifecycleState.Active;
        return false;
    }

    private static string BuildLifecycleLabel(CustomerCardLifecycleState state) => state switch
    {
        CustomerCardLifecycleState.Suspended => "Suspended",
        CustomerCardLifecycleState.Archived => "Archived",
        _ => "Active"
    };

    private MerchantAccountSnapshot ToMerchantSnapshot(MerchantAccount merchant) =>
        new(
            merchant.MerchantId,
            merchant.DisplayName,
            merchant.TownOrCity,
            merchant.Postcode,
            merchant.ContactEmail,
            merchant.CreatedAtUtc,
            merchant.ShopTypeKey);

    private BrandProfileSnapshot ToBrandSnapshot(BrandProfile brand) =>
        new(brand.LogoUrl, brand.PrimaryColor, brand.AccentColor, brand.LogoWidth, brand.LogoHeight);

    private LoyaltyProgrammeSnapshot ToProgrammeSnapshot(LoyaltyProgramme programme) =>
        new(
            programme.ProgrammeId,
            programme.TemplateKey,
            programme.TemplateLabel,
            programme.ProgrammeTypeKey,
            programme.ProgrammeTypeLabel,
            programme.DeliveryTypeKey,
            programme.DeliveryTypeLabel,
            programme.CardTypeKey,
            programme.CardTypeLabel,
            programme.OutputLabel,
            programme.RewardItemLabel,
            programme.RewardThreshold,
            programme.RewardCopy,
            programme.StartsOn,
            programme.EndsOn,
            programme.JoinCode);

    private MerchantSetupChecklistSnapshot BuildSetupChecklist(
        MerchantAccount merchant,
        BrandProfile brand,
        IReadOnlyList<ProgrammeSummarySnapshot> programmeSummaries) =>
        new(
            !string.IsNullOrWhiteSpace(merchant.DisplayName)
            && !string.IsNullOrWhiteSpace(merchant.TownOrCity)
            && !string.IsNullOrWhiteSpace(merchant.Postcode)
            && !string.IsNullOrWhiteSpace(merchant.ContactEmail)
            && !string.IsNullOrWhiteSpace(merchant.ShopTypeKey),
            !string.IsNullOrWhiteSpace(brand.LogoUrl)
            && !string.IsNullOrWhiteSpace(brand.PrimaryColor)
            && !string.IsNullOrWhiteSpace(brand.AccentColor),
            programmeSummaries.Count > 0,
            !string.IsNullOrWhiteSpace(merchant.PasswordHash)
            && !string.IsNullOrWhiteSpace(merchant.PasswordSalt),
            programmeSummaries.Any(summary => !string.IsNullOrWhiteSpace(summary.JoinCode)));

    private static string BuildRewardHeadline(string templateKey, int rewardThreshold, string rewardItemLabel) =>
        string.Equals(templateKey, "coffee-food-offer", StringComparison.OrdinalIgnoreCase)
            ? "20% off food with a coffee"
            : $"Buy {rewardThreshold} {rewardItemLabel}, get one free";

    private LoyaltyProgramme BuildProgrammeFromTemplate(Guid merchantId, ProgrammeTemplateOption template, DateTimeOffset configuredAtUtc)
    {
        var startsOn = DateOnly.FromDateTime(configuredAtUtc.UtcDateTime.Date);
        var endsOn = startsOn.AddDays(DefaultProgrammeDurationDays);

        return new LoyaltyProgramme(
            Guid.NewGuid(),
            merchantId,
            template.TemplateKey,
            template.TemplateLabel,
            template.ProgrammeTypeKey,
            template.ProgrammeTypeLabel,
            template.DeliveryTypeKey,
            template.DeliveryTypeLabel,
            template.CardTypeKey,
            template.CardTypeLabel,
            template.OutputLabel,
            template.RewardItemLabel,
            template.RewardThreshold,
            template.RewardCopy,
            startsOn,
            endsOn,
            $"join-{Guid.NewGuid():N}"[..13],
            configuredAtUtc);
    }

    private ProgrammeTemplateOption? GetProgrammeTemplate(string templateKey) =>
        GetProgrammeTemplates().FirstOrDefault(template =>
            string.Equals(template.TemplateKey, templateKey.Trim(), StringComparison.OrdinalIgnoreCase));

    private string ResolveShopTypeForCreate(string? requestedShopTypeKey)
    {
        var activeShopTypes = GetShopTypes();
        if (activeShopTypes.Count == 0)
        {
            throw new InvalidOperationException("No active shop types are configured.");
        }

        var normalizedRequested = NormalizeCatalogueFilterKey(requestedShopTypeKey);
        if (normalizedRequested is null)
        {
            return activeShopTypes[0].ShopTypeKey;
        }

        if (activeShopTypes.Any(type => string.Equals(type.ShopTypeKey, normalizedRequested, StringComparison.OrdinalIgnoreCase)))
        {
            return normalizedRequested;
        }

        throw new InvalidOperationException($"Unknown or inactive shop type: {requestedShopTypeKey}");
    }

    private string ResolveShopTypeForUpdate(string requestedShopTypeKey, string existingShopTypeKey)
    {
        var normalizedRequested = NormalizeCatalogueFilterKey(requestedShopTypeKey);
        if (normalizedRequested is null)
        {
            return existingShopTypeKey;
        }

        var activeShopTypes = GetShopTypes();
        if (activeShopTypes.Any(type => string.Equals(type.ShopTypeKey, normalizedRequested, StringComparison.OrdinalIgnoreCase)))
        {
            return normalizedRequested;
        }

        throw new InvalidOperationException($"Unknown or inactive shop type: {requestedShopTypeKey}");
    }

    private static string? NormalizeCatalogueFilterKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return key.Trim().ToLowerInvariant();
    }

    private static DateOnly GetTodayUtc() => DateOnly.FromDateTime(DateTime.UtcNow);

    private static bool IsProgrammeActiveOn(LoyaltyProgramme programme, DateOnly onDate) =>
        onDate >= programme.StartsOn && onDate <= programme.EndsOn;

    private static string BuildWalletRegistrationKey(string deviceLibraryIdentifier, string serialNumber) =>
        $"{deviceLibraryIdentifier.Trim()}::{serialNumber.Trim()}";

    private static string BuildWalletUpdateTag(DateTimeOffset updatedAtUtc) =>
        updatedAtUtc.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture);

    private void AppendEvent(Fic.BuildingBlocks.DomainEvent domainEvent, string summary)
    {
        _timeline.Add(new TimelineEventSnapshot(domainEvent.EventId, domainEvent.EventType, summary, domainEvent.OccurredAtUtc));
        Changed?.Invoke();
    }
}

public enum CustomerCardLifecycleState
{
    Active = 0,
    Suspended = 1,
    Archived = 2
}
