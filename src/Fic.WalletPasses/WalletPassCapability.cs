namespace Fic.WalletPasses;

public sealed record WalletPassCapability(
    WalletPassDeliveryMode DeliveryMode,
    string ActionLabel,
    string HelperText,
    IReadOnlyList<string>? DiagnosticItems = null)
{
    public bool SupportsAppleWalletPass => DeliveryMode == WalletPassDeliveryMode.AppleWalletPass;

    public bool HasDiagnostics => DiagnosticItems is { Count: > 0 };
}
