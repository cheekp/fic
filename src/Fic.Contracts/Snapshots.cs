namespace Fic.Contracts;

public sealed record MerchantAccountSnapshot(
    Guid MerchantId,
    string DisplayName,
    string TownOrCity,
    string Postcode,
    string ContactEmail,
    DateTimeOffset CreatedAtUtc);

public sealed record BrandProfileSnapshot(
    string LogoUrl,
    string PrimaryColor,
    string AccentColor,
    int LogoWidth,
    int LogoHeight);

public sealed record LoyaltyProgrammeSnapshot(
    Guid ProgrammeId,
    string TemplateKey,
    string TemplateLabel,
    string ProgrammeTypeKey,
    string ProgrammeTypeLabel,
    string DeliveryTypeKey,
    string DeliveryTypeLabel,
    string OutputLabel,
    string RewardItemLabel,
    int RewardThreshold,
    string RewardCopy,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string JoinCode);

public sealed record ProgrammeSummarySnapshot(
    Guid ProgrammeId,
    string TemplateKey,
    string TemplateLabel,
    string ProgrammeTypeKey,
    string ProgrammeTypeLabel,
    string DeliveryTypeKey,
    string DeliveryTypeLabel,
    string OutputLabel,
    string RewardHeadline,
    string RewardItemLabel,
    int RewardThreshold,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string JoinCode,
    int ActiveCards,
    int RewardsUnlocked);

public sealed record MerchantSetupChecklistSnapshot(
    bool ShopDetailsComplete,
    bool BrandComplete,
    bool HasAnyProgramme,
    bool JoinReady);

public sealed record MerchantInsightsSnapshot(
    int ProgrammesCount,
    int ActiveCards,
    int RewardsUnlocked);

public sealed record WalletCardSnapshot(
    Guid CardId,
    Guid MerchantId,
    Guid ProgrammeId,
    string CardCode,
    string WalletPassId,
    string VendorDisplayName,
    string LogoUrl,
    string RewardItemLabel,
    string RewardCopy,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string PrimaryColor,
    string AccentColor,
    int LogoWidth,
    int LogoHeight,
    int CurrentCount,
    int TargetCount,
    string ProgressDisplayText,
    RewardState RewardState,
    DateTimeOffset LastUpdatedUtc);

public sealed record JoinExperienceSnapshot(
    MerchantAccountSnapshot Merchant,
    BrandProfileSnapshot BrandProfile,
    LoyaltyProgrammeSnapshot Programme);

public sealed record MerchantSummarySnapshot(
    Guid MerchantId,
    string DisplayName,
    string RewardHeadline,
    int ActiveCards,
    int RewardsUnlocked,
    string JoinCode,
    string LogoUrl,
    string PrimaryColor,
    string AccentColor,
    int LogoWidth,
    int LogoHeight);

public sealed record TimelineEventSnapshot(
    Guid EventId,
    string EventType,
    string Summary,
    DateTimeOffset OccurredAtUtc);

public sealed record MerchantWorkspaceSnapshot(
    MerchantAccountSnapshot Merchant,
    BrandProfileSnapshot BrandProfile,
    MerchantSetupChecklistSnapshot SetupChecklist,
    MerchantInsightsSnapshot ShopInsights,
    IReadOnlyList<ProgrammeSummarySnapshot> Programmes,
    LoyaltyProgrammeSnapshot? SelectedProgramme,
    string? SelectedJoinUrl,
    IReadOnlyList<WalletCardSnapshot> SelectedProgrammeCards,
    IReadOnlyList<TimelineEventSnapshot> Timeline);
