using Fic.Contracts;

namespace Fic.Platform.Web.Services;

public sealed record WalletPassDeliverySnapshot(
    WalletCardSnapshot Card,
    string AuthenticationToken);

public enum WalletPassRegistrationStatus
{
    Created,
    Updated,
    NotFound,
    Unauthorized
}

public sealed record WalletPassRegistrationResult(
    WalletPassRegistrationStatus Status);

public sealed record WalletPassSerialUpdateSnapshot(
    IReadOnlyList<string> SerialNumbers,
    string? LastUpdatedTag);

internal sealed record WalletPassDeviceRegistration(
    string DeviceLibraryIdentifier,
    string SerialNumber,
    Guid CardId,
    string PushToken,
    DateTimeOffset RegisteredAtUtc);
