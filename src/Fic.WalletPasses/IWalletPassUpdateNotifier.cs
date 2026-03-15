using Fic.Contracts;

namespace Fic.WalletPasses;

public interface IWalletPassUpdateNotifier
{
    WalletPassPushCapability GetCapability();

    Task<WalletPassUpdateDispatchResult> NotifyPassUpdatedAsync(
        WalletCardSnapshot card,
        IReadOnlyList<string> pushTokens,
        CancellationToken cancellationToken = default);
}
