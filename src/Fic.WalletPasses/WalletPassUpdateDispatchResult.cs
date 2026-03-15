namespace Fic.WalletPasses;

public sealed record WalletPassUpdateDispatchResult(
    int RegistrationCount,
    int SuccessCount,
    int FailureCount,
    bool Skipped,
    string Summary,
    IReadOnlyList<string>? DiagnosticItems = null)
{
    public bool HasDiagnostics => DiagnosticItems is { Count: > 0 };
}
