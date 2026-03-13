using Fic.BuildingBlocks;

namespace Fic.MerchantAccounts;

public sealed record MerchantAccount(
    Guid MerchantId,
    string DisplayName,
    string TownOrCity,
    string Postcode,
    string ContactEmail,
    DateTimeOffset CreatedAtUtc);

public sealed record MerchantAccountCreated(
    Guid MerchantId,
    string DisplayName,
    string TownOrCity,
    string Postcode,
    string ContactEmail,
    DateTimeOffset OccurredAtUtc) : DomainEvent(nameof(MerchantAccountCreated), OccurredAtUtc);

public sealed record MerchantBrandUpdated(
    Guid MerchantId,
    string DisplayName,
    DateTimeOffset OccurredAtUtc) : DomainEvent(nameof(MerchantBrandUpdated), OccurredAtUtc);
