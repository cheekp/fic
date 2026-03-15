namespace Fic.WalletPasses;

public sealed record WalletPassPushCapability(
    bool IsReady,
    string HelperText,
    IReadOnlyList<string>? DiagnosticItems = null)
{
    public bool HasDiagnostics => DiagnosticItems is { Count: > 0 };
}
