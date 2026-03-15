using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Fic.Contracts;
using Fic.WalletPasses;
using Xunit;

namespace Fic.Platform.Web.Tests;

public sealed class AppleWalletPassServiceTests
{
    private const string PngDataUri = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Y1koXUAAAAASUVORK5CYII=";

    [Fact]
    public void GetCapability_ReturnsDiagnostics_WhenSigningIsDisabled()
    {
        var service = new AppleWalletPassService(new AppleWalletPassOptions
        {
            SigningConfigured = false
        });

        var capability = service.GetCapability();

        Assert.False(capability.SupportsAppleWalletPass);
        Assert.Equal("Open Card Preview", capability.ActionLabel);
        Assert.True(capability.HasDiagnostics);
        Assert.Contains("turned off", capability.DiagnosticItems![0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetCapability_ReturnsCertificateDiagnostics_WhenSigningMaterialIsInvalid()
    {
        using var tempDir = new TemporaryDirectory();
        var p12Path = Path.Combine(tempDir.Path, "fic-demo-signing.p12");
        var wwdrPath = Path.Combine(tempDir.Path, "AppleWWDR.cer");

        File.WriteAllBytes(p12Path, [1, 2, 3]);
        File.WriteAllBytes(wwdrPath, [4, 5, 6]);

        var service = new AppleWalletPassService(new AppleWalletPassOptions
        {
            SigningConfigured = true,
            PassTypeIdentifier = "pass.com.fic.demo",
            TeamIdentifier = "ABCDE12345",
            P12CertificatePath = p12Path,
            P12CertificatePassword = "wrong-password",
            WwdrCertificatePath = wwdrPath
        });

        var capability = service.GetCapability();

        Assert.False(capability.SupportsAppleWalletPass);
        Assert.True(capability.HasDiagnostics);
        Assert.Contains(capability.DiagnosticItems!, item => item.Contains("Signing certificate could not be opened", StringComparison.Ordinal));
        Assert.Contains(capability.DiagnosticItems!, item => item.Contains("WWDR certificate could not be opened", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CreatePackageAsync_BuildsSignedPassArchive_WhenCertificatesAreConfigured()
    {
        using var tempDir = new TemporaryDirectory();
        var signingPassword = "fic-demo-password";
        var signingP12Path = Path.Combine(tempDir.Path, "fic-demo-signing.p12");
        var wwdrPath = Path.Combine(tempDir.Path, "AppleWWDR.cer");

        using var signingRsa = RSA.Create(2048);
        var signingRequest = new CertificateRequest(
            "CN=FIC Demo Signing",
            signingRsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        signingRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        signingRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        using var signingCert = signingRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
        File.WriteAllBytes(signingP12Path, signingCert.Export(X509ContentType.Pfx, signingPassword));

        using var wwdrRsa = RSA.Create(2048);
        var wwdrRequest = new CertificateRequest(
            "CN=Apple WWDR Test",
            wwdrRsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        wwdrRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        using var wwdrCert = wwdrRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
        File.WriteAllBytes(wwdrPath, wwdrCert.Export(X509ContentType.Cert));

        var service = new AppleWalletPassService(new AppleWalletPassOptions
        {
            SigningConfigured = true,
            PassTypeIdentifier = "pass.com.fic.demo",
            TeamIdentifier = "ABCDE12345",
            OrganizationName = "FIC Demo",
            Description = "Coffee loyalty card",
            P12CertificatePath = signingP12Path,
            P12CertificatePassword = signingPassword,
            WwdrCertificatePath = wwdrPath
        });

        var package = await service.CreatePackageAsync(
            CreateWalletCard(),
            "auth-demo-token",
            "https://demo.fic.test/wallet/v1/");

        Assert.Equal("application/vnd.apple.pkpass", package.ContentType);
        Assert.EndsWith(".pkpass", package.FileName, StringComparison.Ordinal);

        using var archiveStream = new MemoryStream(package.Bytes);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);

        Assert.NotNull(archive.GetEntry("pass.json"));
        Assert.NotNull(archive.GetEntry("manifest.json"));
        Assert.NotNull(archive.GetEntry("signature"));
        Assert.NotNull(archive.GetEntry("icon.png"));
        Assert.NotNull(archive.GetEntry("logo.png"));

        using var passStream = archive.GetEntry("pass.json")!.Open();
        using var reader = new StreamReader(passStream);
        var passJson = await reader.ReadToEndAsync();
        using var passDocument = JsonDocument.Parse(passJson);

        Assert.Equal("pass.com.fic.demo", passDocument.RootElement.GetProperty("passTypeIdentifier").GetString());
        Assert.Equal("FIC Demo", passDocument.RootElement.GetProperty("organizationName").GetString());
        Assert.Equal("Jo's Coffee", passDocument.RootElement.GetProperty("logoText").GetString());
        Assert.Equal("auth-demo-token", passDocument.RootElement.GetProperty("authenticationToken").GetString());
        Assert.Equal("https://demo.fic.test/wallet/v1/", passDocument.RootElement.GetProperty("webServiceURL").GetString());
    }

    private static WalletCardSnapshot CreateWalletCard() =>
        new(
            Guid.Parse("c03f6e57-3fa7-4a3d-a661-7105619a8ae6"),
            Guid.Parse("7e4f9f3b-6298-4665-93c5-ef3721cb9ab7"),
            Guid.Parse("9325f2fb-3bb9-49b7-a0b8-948852cf00cc"),
            "card-123",
            "wallet-pass-123",
            "Jo's Coffee",
            PngDataUri,
            "coffees",
            "Buy 5 coffees, get one free.",
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30)),
            "#1f3731",
            "#f4c15d",
            160,
            160,
            2,
            5,
            "2/5 coffees",
            RewardState.Locked,
            CustomerCardStatus.Active,
            "Active",
            false,
            DateTimeOffset.UtcNow);

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"fic-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
