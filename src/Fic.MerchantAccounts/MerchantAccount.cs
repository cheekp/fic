using Fic.BuildingBlocks;

namespace Fic.MerchantAccounts;

public sealed record MerchantAccount(
    Guid MerchantId,
    string DisplayName,
    string TownOrCity,
    string Postcode,
    string ContactEmail,
    DateTimeOffset CreatedAtUtc,
    string ShopTypeKey = "coffee",
    string? PasswordHash = null,
    string? PasswordSalt = null);

public sealed record MerchantAccountCreated(
    Guid MerchantId,
    string DisplayName,
    string TownOrCity,
    string Postcode,
    string ContactEmail,
    string ShopTypeKey,
    DateTimeOffset OccurredAtUtc) : DomainEvent(nameof(MerchantAccountCreated), OccurredAtUtc);

public sealed record MerchantBrandUpdated(
    Guid MerchantId,
    string DisplayName,
    DateTimeOffset OccurredAtUtc) : DomainEvent(nameof(MerchantBrandUpdated), OccurredAtUtc);
