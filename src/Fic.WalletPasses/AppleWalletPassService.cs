using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fic.Contracts;
using Fic.MerchantAccounts;

namespace Fic.WalletPasses;

public sealed class AppleWalletPassService(
    AppleWalletPassOptions options,
    IMerchantBrandAssetStore? brandAssetStore = null) : IAppleWalletPassService
{
    private const string PassContentType = "application/vnd.apple.pkpass";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private static readonly byte[] FallbackImageBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Y1koXUAAAAASUVORK5CYII=");

    public WalletPassCapability GetCapability()
    {
        return EvaluateCapability();
    }

    public async Task<WalletPassPackage> CreatePackageAsync(
        WalletCardSnapshot card,
        string authenticationToken,
        string webServiceUrl,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var capability = EvaluateCapability();
        if (!capability.SupportsAppleWalletPass)
        {
            throw new InvalidOperationException(capability.HelperText);
        }

        var packageFiles = await BuildUnsignedPackageFilesAsync(card, authenticationToken, webServiceUrl, cancellationToken);
        var manifestBytes = BuildManifest(packageFiles);
        var signatureBytes = BuildSignature(manifestBytes);

        packageFiles["manifest.json"] = manifestBytes;
        packageFiles["signature"] = signatureBytes;

        return new WalletPassPackage(
            BuildFileName(card),
            PassContentType,
            BuildZipArchive(packageFiles));
    }

    private WalletPassCapability EvaluateCapability()
    {
        var diagnostics = new List<string>();

        if (!options.SigningConfigured)
        {
            diagnostics.Add("Apple Wallet signing is turned off for this environment.");
        }

        if (string.IsNullOrWhiteSpace(options.PassTypeIdentifier)
            || string.IsNullOrWhiteSpace(options.TeamIdentifier))
        {
            if (string.IsNullOrWhiteSpace(options.PassTypeIdentifier))
            {
                diagnostics.Add("Pass type identifier is missing.");
            }

            if (string.IsNullOrWhiteSpace(options.TeamIdentifier))
            {
                diagnostics.Add("Team identifier is missing.");
            }
        }

        if (string.IsNullOrWhiteSpace(options.P12CertificatePath))
        {
            diagnostics.Add("Signing certificate path is missing.");
        }
        else if (!File.Exists(options.P12CertificatePath))
        {
            diagnostics.Add("Signing certificate file was not found.");
        }
        else if (!TryLoadSigningCertificate(out _, out var signingIssue))
        {
            diagnostics.Add(signingIssue!);
        }

        if (string.IsNullOrWhiteSpace(options.WwdrCertificatePath))
        {
            diagnostics.Add("Apple WWDR certificate path is missing.");
        }
        else if (!File.Exists(options.WwdrCertificatePath))
        {
            diagnostics.Add("Apple WWDR certificate file was not found.");
        }
        else if (!TryLoadWwdrCertificate(out _, out var wwdrIssue))
        {
            diagnostics.Add(wwdrIssue!);
        }

        if (diagnostics.Count == 0)
        {
            return new WalletPassCapability(
                WalletPassDeliveryMode.AppleWalletPass,
                "Add to Apple Wallet",
                "Downloads a signed Apple Wallet pass from this join flow.");
        }

        var helperText = options.SigningConfigured
            ? "Finish the Apple Wallet signing checklist to issue a real pass."
            : "Apple Wallet signing is turned off, so this environment stays in preview mode.";

        return new WalletPassCapability(
            WalletPassDeliveryMode.Preview,
            "Open Card Preview",
            helperText,
            diagnostics);
    }

    private async Task<Dictionary<string, byte[]>> BuildUnsignedPackageFilesAsync(
        WalletCardSnapshot card,
        string authenticationToken,
        string webServiceUrl,
        CancellationToken cancellationToken)
    {
        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            ["pass.json"] = JsonSerializer.SerializeToUtf8Bytes(
                BuildPassDocument(card, authenticationToken, webServiceUrl),
                JsonOptions),
            ["icon.png"] = LoadDefaultAssetBytes("icon.png"),
            ["icon@2x.png"] = LoadDefaultAssetBytes("icon@2x.png"),
            ["icon@3x.png"] = LoadDefaultAssetBytes("icon@3x.png")
        };

        if (TryDecodePngDataUri(card.LogoUrl, out var logoBytes))
        {
            files["logo.png"] = logoBytes;
            files["logo@2x.png"] = logoBytes;
            files["logo@3x.png"] = logoBytes;
        }
        else if (brandAssetStore is not null)
        {
            var asset = await brandAssetStore.GetAssetAsync(card.LogoUrl, cancellationToken);
            if (asset is not null && string.Equals(asset.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
            {
                files["logo.png"] = asset.Bytes;
                files["logo@2x.png"] = asset.Bytes;
                files["logo@3x.png"] = asset.Bytes;
            }
        }

        return files;
    }

    private AppleWalletPassDocument BuildPassDocument(
        WalletCardSnapshot card,
        string authenticationToken,
        string webServiceUrl)
    {
        var theme = MerchantBrandThemeCompiler.Compile(
            card.PrimaryColor,
            card.AccentColor,
            new MerchantLogoMetadata(card.LogoWidth, card.LogoHeight));
        var background = ToAppleRgb(theme.CanvasEndColor);
        var foreground = ToAppleRgb(theme.InkColor);
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
            AuthenticationToken = authenticationToken,
            WebServiceUrl = webServiceUrl,
            LogoText = card.VendorDisplayName,
            BackgroundColor = background,
            ForegroundColor = foreground,
            LabelColor = ToAppleRgb(theme.AccentInkColor),
            Barcode = new AppleWalletBarcode
            {
                Format = "PKBarcodeFormatQR",
                Message = card.CardCode,
                MessageEncoding = "iso-8859-1",
                AltText = card.CardCode
            },
            Barcodes =
            [
                new AppleWalletBarcode
                {
                    Format = "PKBarcodeFormatQR",
                    Message = card.CardCode,
                    MessageEncoding = "iso-8859-1",
                    AltText = card.CardCode
                }
            ],
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
                        Key = "valid-from",
                        Label = "VALID FROM",
                        Value = card.StartsOn.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
                    },
                    new AppleWalletField
                    {
                        Key = "expires-on",
                        Label = "EXPIRES",
                        Value = card.EndsOn.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
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

    private byte[] LoadDefaultAssetBytes(string fileName)
    {
        if (!string.IsNullOrWhiteSpace(options.DefaultAssetDirectory))
        {
            var candidatePath = Path.Combine(options.DefaultAssetDirectory, fileName);
            if (File.Exists(candidatePath))
            {
                return File.ReadAllBytes(candidatePath);
            }
        }

        return FallbackImageBytes;
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
        var signingCertificate = LoadSigningCertificate();
        var wwdrCertificate = LoadWwdrCertificate();

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

    private X509Certificate2 LoadSigningCertificate()
    {
        if (!TryLoadSigningCertificate(out var certificate, out var issue))
        {
            throw new InvalidOperationException(issue);
        }

        return certificate!;
    }

    private bool TryLoadSigningCertificate(out X509Certificate2? certificate, out string? issue)
    {
        try
        {
            certificate = LoadPkcs12Certificate(
                options.P12CertificatePath,
                options.P12CertificatePassword);

            if (!certificate.HasPrivateKey)
            {
                issue = "Signing certificate does not include a private key.";
                certificate.Dispose();
                certificate = null;
                return false;
            }

            issue = null;
            return true;
        }
        catch (CryptographicException)
        {
            certificate = null;
            issue = "Signing certificate could not be opened. Check the .p12 path and password.";
            return false;
        }
        catch (IOException)
        {
            certificate = null;
            issue = "Signing certificate could not be opened. Check the .p12 path and password.";
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            certificate = null;
            issue = "Signing certificate could not be opened. Check the .p12 path and password.";
            return false;
        }
    }

    private X509Certificate2 LoadWwdrCertificate()
    {
        if (!TryLoadWwdrCertificate(out var certificate, out var issue))
        {
            throw new InvalidOperationException(issue);
        }

        return certificate!;
    }

    private bool TryLoadWwdrCertificate(out X509Certificate2? certificate, out string? issue)
    {
        try
        {
            certificate = X509CertificateLoader.LoadCertificateFromFile(options.WwdrCertificatePath);
            issue = null;
            return true;
        }
        catch (CryptographicException)
        {
            certificate = null;
            issue = "Apple WWDR certificate could not be opened.";
            return false;
        }
        catch (IOException)
        {
            certificate = null;
            issue = "Apple WWDR certificate could not be opened.";
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            certificate = null;
            issue = "Apple WWDR certificate could not be opened.";
            return false;
        }
    }

    private static X509Certificate2 LoadPkcs12Certificate(string path, string password)
    {
        const X509KeyStorageFlags baseFlags = X509KeyStorageFlags.Exportable;

        try
        {
            return X509CertificateLoader.LoadPkcs12FromFile(
                path,
                password,
                baseFlags | X509KeyStorageFlags.EphemeralKeySet);
        }
        catch (PlatformNotSupportedException)
        {
            return X509CertificateLoader.LoadPkcs12FromFile(
                path,
                password,
                baseFlags);
        }
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

        [JsonPropertyName("authenticationToken")]
        public string AuthenticationToken { get; init; } = string.Empty;

        [JsonPropertyName("webServiceURL")]
        public string WebServiceUrl { get; init; } = string.Empty;

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

        [JsonPropertyName("barcodes")]
        public AppleWalletBarcode[]? Barcodes { get; init; }

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
