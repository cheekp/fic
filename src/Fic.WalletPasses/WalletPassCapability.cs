namespace Fic.WalletPasses;

public sealed record WalletPassCapability(
    WalletPassDeliveryMode DeliveryMode,
    string ActionLabel,
    string HelperText)
{
    public bool SupportsAppleWalletPass => DeliveryMode == WalletPassDeliveryMode.AppleWalletPass;
}
