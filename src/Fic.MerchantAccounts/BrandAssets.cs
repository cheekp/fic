namespace Fic.MerchantAccounts;

public sealed record MerchantLogoUpload(
    byte[] Bytes,
    string ContentType,
    string FileName);

public interface IMerchantBrandAssetStore
{
    Task<string> SaveLogoAsync(
        Guid merchantId,
        MerchantLogoUpload upload,
        CancellationToken cancellationToken = default);
}
