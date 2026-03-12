using System.Buffers.Binary;

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

public static class MerchantBrandAssetInspector
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static void EnsureValidPng(byte[] bytes)
    {
        if (!LooksLikePng(bytes))
        {
            throw new InvalidOperationException("Merchant logo upload does not contain a valid PNG signature.");
        }
    }

    public static MerchantLogoMetadata ReadLogoMetadata(byte[] bytes)
    {
        EnsureValidPng(bytes);

        if (bytes.Length < 24 || !bytes.AsSpan(12, 4).SequenceEqual("IHDR"u8))
        {
            throw new InvalidOperationException("Merchant logo upload is missing a readable PNG IHDR header.");
        }

        var width = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4));
        var height = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4));
        if (width <= 0 || height <= 0)
        {
            throw new InvalidOperationException("Merchant logo upload reported invalid PNG dimensions.");
        }

        return new MerchantLogoMetadata(width, height);
    }

    private static bool LooksLikePng(byte[] bytes) =>
        bytes.Length >= PngSignature.Length && PngSignature.SequenceEqual(bytes.Take(PngSignature.Length));
}

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
