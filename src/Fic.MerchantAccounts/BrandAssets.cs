namespace Fic.MerchantAccounts;

public static class MerchantBrandAssetDefaults
{
    public const string PublicRequestPath = "/merchant-brand-assets";
    public const string BlobContainerName = "merchant-brand-assets";
}

public sealed record MerchantLogoUpload(
    byte[] Bytes,
    string ContentType,
    string FileName);

public sealed record MerchantBrandAsset(
    byte[] Bytes,
    string ContentType);

public interface IMerchantBrandAssetStore
{
    Task<string> SaveLogoAsync(
        Guid merchantId,
        MerchantLogoUpload upload,
        CancellationToken cancellationToken = default);

    Task<MerchantBrandAsset?> GetAssetAsync(
        string publicPath,
        CancellationToken cancellationToken = default);
}
