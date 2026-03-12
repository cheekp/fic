using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fic.Contracts;

namespace Fic.WalletPasses;

public sealed class AppleWalletPassService(AppleWalletPassOptions options) : IAppleWalletPassService
{
    private const string PassContentType = "application/vnd.apple.pkpass";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private static readonly byte[] FallbackIconBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Y1koXUAAAAASUVORK5CYII=");

    public WalletPassCapability GetCapability()
    {
        return CanIssueSignedPass(out var reason)
            ? new WalletPassCapability(
                WalletPassDeliveryMode.AppleWalletPass,
                "Add to Apple Wallet",
                "Downloads a signed Apple Wallet pass from this join flow.")
            : new WalletPassCapability(
                WalletPassDeliveryMode.Preview,
                "Open Card Preview",
                reason ?? "Apple Wallet signing is not configured in this environment.");
    }

    public Task<WalletPassPackage> CreatePackageAsync(WalletCardSnapshot card, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!CanIssueSignedPass(out var reason))
        {
            throw new InvalidOperationException(reason);
        }

        var packageFiles = BuildUnsignedPackageFiles(card);
        var manifestBytes = BuildManifest(packageFiles);
        var signatureBytes = BuildSignature(manifestBytes);

        packageFiles["manifest.json"] = manifestBytes;
        packageFiles["signature"] = signatureBytes;

        return Task.FromResult(new WalletPassPackage(
            BuildFileName(card),
            PassContentType,
            BuildZipArchive(packageFiles)));
    }

    private bool CanIssueSignedPass(out string? reason)
    {
        if (!options.SigningConfigured)
        {
            reason = "Apple Wallet signing is turned off, so this environment stays in preview mode.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.PassTypeIdentifier)
            || string.IsNullOrWhiteSpace(options.TeamIdentifier))
        {
            reason = "Pass type identifier and team identifier must be configured before issuing a real pass.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.P12CertificatePath) || !File.Exists(options.P12CertificatePath))
        {
            reason = "The Apple Wallet signing certificate path is missing or invalid.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.WwdrCertificatePath) || !File.Exists(options.WwdrCertificatePath))
        {
            reason = "The Apple WWDR certificate path is missing or invalid.";
            return false;
        }

        reason = null;
        return true;
    }

    private Dictionary<string, byte[]> BuildUnsignedPackageFiles(WalletCardSnapshot card)
    {
        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            ["pass.json"] = JsonSerializer.SerializeToUtf8Bytes(BuildPassDocument(card), JsonOptions),
            ["icon.png"] = LoadDefaultIconBytes(),
            ["icon@2x.png"] = LoadDefaultIconBytes()
        };

        if (TryDecodePngDataUri(card.LogoUrl, out var logoBytes))
        {
            files["logo.png"] = logoBytes;
            files["logo@2x.png"] = logoBytes;
        }

        return files;
    }

    private AppleWalletPassDocument BuildPassDocument(WalletCardSnapshot card)
    {
        var background = ToAppleRgb(card.PrimaryColor);
        var foreground = PickReadableForeground(card.PrimaryColor);
        var rewardHeadline = $"Buy {card.TargetCount} {card.RewardItemLabel}, get one free";

        return new AppleWalletPassDocument
        {
            FormatVersion = 1,
            PassTypeIdentifier = options.PassTypeIdentifier,
            SerialNumber = card.WalletPassId,
            TeamIdentifier = options.TeamIdentifier,
            OrganizationName = string.IsNullOrWhiteSpace(options.OrganizationName)
                ? card.VendorDisplayName
                : options.OrganizationName,
            Description = string.IsNullOrWhiteSpace(options.Description)
                ? $"{card.VendorDisplayName} loyalty card"
                : options.Description,
            LogoText = card.VendorDisplayName,
            BackgroundColor = background,
            ForegroundColor = foreground,
            LabelColor = foreground,
            Barcode = new AppleWalletBarcode
            {
                Format = "PKBarcodeFormatQR",
                Message = card.CardCode,
                MessageEncoding = "iso-8859-1",
                AltText = card.CardCode
            },
            StoreCard = new AppleWalletFieldSet
            {
                PrimaryFields =
                [
                    new AppleWalletField
                    {
                        Key = "progress",
                        Label = "STAMPS",
                        Value = card.ProgressDisplayText
                    }
                ],
                SecondaryFields =
                [
                    new AppleWalletField
                    {
                        Key = "reward",
                        Label = "REWARD",
                        Value = rewardHeadline
                    },
                    new AppleWalletField
                    {
                        Key = "status",
                        Label = "STATUS",
                        Value = card.RewardState.ToString()
                    }
                ],
                AuxiliaryFields =
                [
                    new AppleWalletField
                    {
                        Key = "vendor",
                        Label = "BUSINESS",
                        Value = card.VendorDisplayName
                    }
                ],
                BackFields =
                [
                    new AppleWalletField
                    {
                        Key = "details",
                        Label = "DETAILS",
                        Value = card.RewardCopy
                    },
                    new AppleWalletField
                    {
                        Key = "scan-code",
                        Label = "PASS CODE",
                        Value = card.CardCode
                    }
                ]
            }
        };
    }

    private byte[] LoadDefaultIconBytes()
    {
        return !string.IsNullOrWhiteSpace(options.DefaultIconPath) && File.Exists(options.DefaultIconPath)
            ? File.ReadAllBytes(options.DefaultIconPath)
            : FallbackIconBytes;
    }

    private byte[] BuildManifest(IReadOnlyDictionary<string, byte[]> files)
    {
        var manifest = files
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .ToDictionary(
                x => x.Key,
                x => Convert.ToHexString(SHA1.HashData(x.Value)).ToLowerInvariant(),
                StringComparer.Ordinal);

        return JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions);
    }

    private byte[] BuildSignature(byte[] manifestBytes)
    {
        var signingCertificate = X509CertificateLoader.LoadPkcs12FromFile(
            options.P12CertificatePath,
            options.P12CertificatePassword,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        var wwdrCertificate = X509CertificateLoader.LoadCertificateFromFile(options.WwdrCertificatePath);

        var signedCms = new SignedCms(new ContentInfo(manifestBytes), detached: true);
        var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signingCertificate)
        {
            IncludeOption = X509IncludeOption.EndCertOnly
        };
        signer.Certificates.Add(wwdrCertificate);
        signer.SignedAttributes.Add(new Pkcs9SigningTime(DateTime.UtcNow));

        signedCms.ComputeSignature(signer);
        return signedCms.Encode();
    }

    private static byte[] BuildZipArchive(IReadOnlyDictionary<string, byte[]> files)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in files.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                var entry = archive.CreateEntry(file.Key, CompressionLevel.SmallestSize);
                using var entryStream = entry.Open();
                entryStream.Write(file.Value);
            }
        }

        return stream.ToArray();
    }

    private static string BuildFileName(WalletCardSnapshot card)
    {
        var safeVendor = string.Concat(card.VendorDisplayName.Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '-'))
            .Trim('-');
        return $"{safeVendor}-{card.WalletPassId}.pkpass";
    }

    private static string ToAppleRgb(string hexColor)
    {
        var normalized = hexColor.Trim().TrimStart('#');
        if (normalized.Length != 6)
        {
            return "rgb(31, 55, 49)";
        }

        var red = int.Parse(normalized[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var green = int.Parse(normalized[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var blue = int.Parse(normalized[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return $"rgb({red}, {green}, {blue})";
    }

    private static string PickReadableForeground(string hexColor)
    {
        var normalized = hexColor.Trim().TrimStart('#');
        if (normalized.Length != 6)
        {
            return "rgb(255, 255, 255)";
        }

        var red = int.Parse(normalized[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var green = int.Parse(normalized[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var blue = int.Parse(normalized[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var luminance = (0.299 * red) + (0.587 * green) + (0.114 * blue);
        return luminance >= 160 ? "rgb(17, 24, 39)" : "rgb(255, 248, 227)";
    }

    private static bool TryDecodePngDataUri(string value, out byte[] bytes)
    {
        const string prefix = "data:image/png;base64,";

        if (!value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            bytes = [];
            return false;
        }

        try
        {
            bytes = Convert.FromBase64String(value[prefix.Length..]);
            return true;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }

    private sealed class AppleWalletPassDocument
    {
        [JsonPropertyName("formatVersion")]
        public int FormatVersion { get; init; }

        [JsonPropertyName("passTypeIdentifier")]
        public string PassTypeIdentifier { get; init; } = string.Empty;

        [JsonPropertyName("serialNumber")]
        public string SerialNumber { get; init; } = string.Empty;

        [JsonPropertyName("teamIdentifier")]
        public string TeamIdentifier { get; init; } = string.Empty;

        [JsonPropertyName("organizationName")]
        public string OrganizationName { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("logoText")]
        public string LogoText { get; init; } = string.Empty;

        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; init; } = string.Empty;

        [JsonPropertyName("foregroundColor")]
        public string ForegroundColor { get; init; } = string.Empty;

        [JsonPropertyName("labelColor")]
        public string LabelColor { get; init; } = string.Empty;

        [JsonPropertyName("barcode")]
        public AppleWalletBarcode Barcode { get; init; } = new();

        [JsonPropertyName("storeCard")]
        public AppleWalletFieldSet StoreCard { get; init; } = new();
    }

    private sealed class AppleWalletBarcode
    {
        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("format")]
        public string Format { get; init; } = string.Empty;

        [JsonPropertyName("messageEncoding")]
        public string MessageEncoding { get; init; } = string.Empty;

        [JsonPropertyName("altText")]
        public string AltText { get; init; } = string.Empty;
    }

    private sealed class AppleWalletFieldSet
    {
        [JsonPropertyName("primaryFields")]
        public AppleWalletField[]? PrimaryFields { get; init; }

        [JsonPropertyName("secondaryFields")]
        public AppleWalletField[]? SecondaryFields { get; init; }

        [JsonPropertyName("auxiliaryFields")]
        public AppleWalletField[]? AuxiliaryFields { get; init; }

        [JsonPropertyName("backFields")]
        public AppleWalletField[]? BackFields { get; init; }
    }

    private sealed class AppleWalletField
    {
        [JsonPropertyName("key")]
        public string Key { get; init; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; init; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; init; } = string.Empty;
    }
}
