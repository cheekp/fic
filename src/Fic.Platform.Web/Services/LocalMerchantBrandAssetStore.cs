using Fic.MerchantAccounts;

namespace Fic.Platform.Web.Services;

public sealed class LocalMerchantBrandAssetStore(IWebHostEnvironment environment) : IMerchantBrandAssetStore
{
    private const string PublicRequestPath = "/merchant-brand-assets";
    private readonly string _rootPath = Path.Combine(environment.ContentRootPath, "App_Data", "merchant-brand-assets");

    public async Task<string> SaveLogoAsync(
        Guid merchantId,
        MerchantLogoUpload upload,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.Equals(upload.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Merchant logo uploads must be PNG files.");
        }

        if (!LooksLikePng(upload.Bytes))
        {
            throw new InvalidOperationException("Merchant logo upload does not contain a valid PNG signature.");
        }

        var merchantDirectory = Path.Combine(_rootPath, merchantId.ToString("N"));
        Directory.CreateDirectory(merchantDirectory);

        var filePath = Path.Combine(merchantDirectory, "logo.png");
        await File.WriteAllBytesAsync(filePath, upload.Bytes, cancellationToken);

        return $"{PublicRequestPath}/{merchantId:N}/logo.png";
    }

    private static bool LooksLikePng(byte[] bytes)
    {
        byte[] pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        return bytes.Length >= pngSignature.Length && pngSignature.SequenceEqual(bytes.Take(pngSignature.Length));
    }
}
