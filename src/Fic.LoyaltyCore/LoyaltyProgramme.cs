using Fic.BuildingBlocks;
using Fic.Contracts;

namespace Fic.LoyaltyCore;

public sealed record BrandProfile(
    string LogoUrl,
    string PrimaryColor,
    string AccentColor,
    int LogoWidth,
    int LogoHeight);

public sealed record LoyaltyProgramme(
    Guid ProgrammeId,
    Guid MerchantId,
    string RewardItemLabel,
    int RewardThreshold,
    string RewardCopy,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string JoinCode,
    DateTimeOffset ConfiguredAtUtc);

public sealed record CustomerCard(
    Guid CardId,
    Guid MerchantId,
    Guid ProgrammeId,
    string CardCode,
    string WalletPassId,
    DateTimeOffset JoinedAtUtc);

public sealed record CustomerProgress(
    Guid CardId,
    int CurrentCount,
    int TargetCount,
    string ProgressDisplayText,
    RewardState RewardState,
    DateTimeOffset LastUpdatedUtc);

public sealed record ProgrammeConfigured(
    Guid MerchantId,
    Guid ProgrammeId,
    string RewardItemLabel,
    int RewardThreshold,
    DateTimeOffset OccurredAtUtc) : DomainEvent(nameof(ProgrammeConfigured), OccurredAtUtc);

public sealed record CardTemplateUpdated(
    Guid MerchantId,
    Guid ProgrammeId,
    string RewardItemLabel,
    int RewardThreshold,
    DateTimeOffset OccurredAtUtc) : DomainEvent(nameof(CardTemplateUpdated), OccurredAtUtc);

public sealed record CustomerJoined(
    Guid MerchantId,
    Guid ProgrammeId,
    Guid CardId,
    string CardCode,
    DateTimeOffset OccurredAtUtc) : DomainEvent(nameof(CustomerJoined), OccurredAtUtc);

public sealed record VisitAwarded(
    Guid MerchantId,
    Guid CardId,
    int CurrentCount,
    int TargetCount,
    DateTimeOffset OccurredAtUtc) : DomainEvent(nameof(VisitAwarded), OccurredAtUtc);

public sealed record RewardRedeemed(
    Guid MerchantId,
    Guid CardId,
    DateTimeOffset OccurredAtUtc) : DomainEvent(nameof(RewardRedeemed), OccurredAtUtc);
