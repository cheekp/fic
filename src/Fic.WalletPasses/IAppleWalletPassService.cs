using Fic.Contracts;

namespace Fic.WalletPasses;

public interface IAppleWalletPassService
{
    WalletPassCapability GetCapability();

    Task<WalletPassPackage> CreatePackageAsync(
        WalletCardSnapshot card,
        string authenticationToken,
        string webServiceUrl,
        CancellationToken cancellationToken = default);
}
