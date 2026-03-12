namespace Fic.Contracts;

public sealed record MerchantAccountSnapshot(
    Guid MerchantId,
    string DisplayName,
    string ContactEmail,
    DateTimeOffset CreatedAtUtc);

public sealed record BrandProfileSnapshot(
    string LogoUrl,
    string PrimaryColor,
    string AccentColor);

public sealed record LoyaltyProgrammeSnapshot(
    Guid ProgrammeId,
    string RewardItemLabel,
    int RewardThreshold,
    string RewardCopy,
    string JoinCode);

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
    string PrimaryColor,
    string AccentColor,
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
    string JoinCode);

public sealed record TimelineEventSnapshot(
    Guid EventId,
    string EventType,
    string Summary,
    DateTimeOffset OccurredAtUtc);

public sealed record MerchantWorkspaceSnapshot(
    MerchantAccountSnapshot Merchant,
    BrandProfileSnapshot BrandProfile,
    LoyaltyProgrammeSnapshot Programme,
    string JoinUrl,
    IReadOnlyList<WalletCardSnapshot> Cards,
    IReadOnlyList<TimelineEventSnapshot> Timeline);
