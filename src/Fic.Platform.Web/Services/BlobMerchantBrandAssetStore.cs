using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fic.MerchantAccounts;

namespace Fic.Platform.Web.Services;

public sealed class BlobMerchantBrandAssetStore(
    BlobServiceClient blobServiceClient,
    IConfiguration configuration) : IMerchantBrandAssetStore
{
    private readonly BlobContainerClient _container = blobServiceClient.GetBlobContainerClient(
        configuration.GetValue("BrandAssets:BlobContainerName", MerchantBrandAssetDefaults.BlobContainerName));

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

        await _container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blobName = $"{merchantId:N}/logo.png";
        var blobClient = _container.GetBlobClient(blobName);

        await using var stream = new MemoryStream(upload.Bytes);
        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/png"
                }
            },
            cancellationToken);

        return $"{MerchantBrandAssetDefaults.PublicRequestPath}/{blobName}";
    }

    public async Task<MerchantBrandAsset?> GetAssetAsync(
        string publicPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryMapToBlobName(publicPath, out var blobName))
        {
            return null;
        }

        var blobClient = _container.GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var download = await blobClient.DownloadContentAsync(cancellationToken);
        return new MerchantBrandAsset(download.Value.Content.ToArray(), download.Value.Details.ContentType ?? "image/png");
    }

    private static bool TryMapToBlobName(string publicPath, out string blobName)
    {
        blobName = string.Empty;

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

        blobName = string.Join('/', relativeSegments);
        return blobName.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
    }
}
