namespace Fic.WalletPasses;

public sealed record WalletPassUpdateDispatchResult(
    int RegistrationCount,
    int SuccessCount,
    int FailureCount,
    bool Skipped,
    string Summary,
    IReadOnlyList<string>? DiagnosticItems = null,
    int RetryableFailureCount = 0,
    int PermanentFailureCount = 0,
    IReadOnlyList<string>? InvalidatedPushTokens = null)
{
    public bool HasDiagnostics => DiagnosticItems is { Count: > 0 };

    public bool HasInvalidatedPushTokens => InvalidatedPushTokens is { Count: > 0 };
}
