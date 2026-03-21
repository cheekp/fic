using Fic.MerchantAccounts;

namespace Fic.Platform.Web.Services;

public sealed class LocalMerchantBrandAssetStore(IWebHostEnvironment environment) : IMerchantBrandAssetStore
{
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

        MerchantBrandAssetInspector.EnsureValidPng(upload.Bytes);

        var merchantDirectory = Path.Combine(_rootPath, merchantId.ToString("N"));
        Directory.CreateDirectory(merchantDirectory);

        foreach (var existingLogo in Directory.EnumerateFiles(merchantDirectory, "*.png"))
        {
            File.Delete(existingLogo);
        }

        var fileName = $"logo-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png";
        var filePath = Path.Combine(merchantDirectory, fileName);
        await File.WriteAllBytesAsync(filePath, upload.Bytes, cancellationToken);

        return $"{MerchantBrandAssetDefaults.PublicRequestPath}/{merchantId:N}/{fileName}";
    }

    public async Task<MerchantBrandAsset?> GetAssetAsync(
        string publicPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryMapToFilePath(publicPath, out var filePath) || !File.Exists(filePath))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        return new MerchantBrandAsset(bytes, "image/png");
    }

    private bool TryMapToFilePath(string publicPath, out string filePath)
    {
        filePath = string.Empty;

        var assetPath = publicPath;
        if (Uri.TryCreate(publicPath, UriKind.Absolute, out var absoluteUri))
        {
            assetPath = absoluteUri.AbsolutePath;
        }

        if (!assetPath.StartsWith(MerchantBrandAssetDefaults.PublicRequestPath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = assetPath[MerchantBrandAssetDefaults.PublicRequestPath.Length..].TrimStart('/');
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        var relativeSegments = relativePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (relativeSegments.Length == 0 || relativeSegments.Any(segment => segment == ".."))
        {
            return false;
        }

        var candidatePath = Path.GetFullPath(Path.Combine(_rootPath, Path.Combine(relativeSegments)));
        var rootPath = Path.GetFullPath(_rootPath);

        if (!candidatePath.StartsWith(rootPath, StringComparison.Ordinal))
        {
            return false;
        }

        if (!candidatePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        filePath = candidatePath;
        return true;
    }
}
