namespace Fic.WalletPasses;

public sealed class AppleWalletPassOptions
{
    public bool SigningConfigured { get; set; }

    public bool PushNotificationsEnabled { get; set; }

    public string PassTypeIdentifier { get; set; } = string.Empty;

    public string TeamIdentifier { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;

    public string Description { get; set; } = "Loyalty card";

    public string P12CertificatePath { get; set; } = string.Empty;

    public string P12CertificatePassword { get; set; } = string.Empty;

    public string WwdrCertificatePath { get; set; } = string.Empty;

    public string DefaultAssetDirectory { get; set; } = string.Empty;

    public bool UseSandboxPushEndpoint { get; set; } = true;

    public string PushEndpointOverride { get; set; } = string.Empty;
}
