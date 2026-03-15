using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Fic.Platform.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Fic.Platform.Web.Tests;

public sealed class WalletPassWebServiceTests
{
    private const string FallbackLogoUrl = "data:image/svg+xml;base64,ZmFrZQ==";
    private const string PassTypeIdentifier = "pass.com.fic.demo";

    [Fact]
    public async Task WalletWebService_RegistersUpdatedSerial_AndReturnsRefreshedPass()
    {
        await using var factory = new WalletPassWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();

        var workspace = await state.CreateMerchantAsync(
            "Jo's Coffee",
            "Bristol",
            "BS1 4DJ",
            "owner@joscoffee.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: "#1f3731",
            accentColor: "#f4c15d",
            baseUri: client.BaseAddress!.ToString());
        workspace = state.CreateProgramme(workspace.Merchant.MerchantId, "coffee-visits", client.BaseAddress!.ToString())!;
        var selectedProgramme = Assert.IsType<Fic.Contracts.LoyaltyProgrammeSnapshot>(workspace.SelectedProgramme);

        var card = state.JoinCustomer(selectedProgramme.JoinCode)!;
        var delivery = state.GetWalletPassDelivery(card.CardId)!;
        var baseline = delivery.Card.LastUpdatedUtc.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApplePass", delivery.AuthenticationToken);

        var registerResponse = await client.PostAsync(
            $"/wallet/v1/devices/device-demo-1/registrations/{PassTypeIdentifier}/{delivery.Card.WalletPassId}",
            new StringContent("{\"pushToken\":\"push-token-demo-1\"}", Encoding.UTF8, "application/json"));

        Assert.True(
            registerResponse.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created but got {(int)registerResponse.StatusCode}: {await registerResponse.Content.ReadAsStringAsync()}");

        var updated = state.AwardVisit(
            workspace.Merchant.MerchantId,
            selectedProgramme.ProgrammeId,
            delivery.Card.CardCode,
            client.BaseAddress!.ToString());

        Assert.NotNull(updated);

        var updatesResponse = await client.GetAsync(
            $"/wallet/v1/devices/device-demo-1/registrations/{PassTypeIdentifier}?passesUpdatedSince={baseline}");

        Assert.Equal(HttpStatusCode.OK, updatesResponse.StatusCode);

        var updates = await updatesResponse.Content.ReadFromJsonAsync<WalletPassSerialUpdatePayload>();
        Assert.NotNull(updates);
        Assert.Contains(delivery.Card.WalletPassId, updates!.SerialNumbers);
        Assert.False(string.IsNullOrWhiteSpace(updates.LastUpdated));

        var passResponse = await client.GetAsync($"/wallet/v1/passes/{PassTypeIdentifier}/{delivery.Card.WalletPassId}");

        Assert.Equal(HttpStatusCode.OK, passResponse.StatusCode);
        Assert.Equal("application/vnd.apple.pkpass", passResponse.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(passResponse.Content.Headers.LastModified);

        using var passStream = new MemoryStream(await passResponse.Content.ReadAsByteArrayAsync());
        using var archive = new ZipArchive(passStream, ZipArchiveMode.Read);
        using var passJsonStream = archive.GetEntry("pass.json")!.Open();
        using var reader = new StreamReader(passJsonStream);
        using var document = JsonDocument.Parse(await reader.ReadToEndAsync());

        Assert.Equal(
            "1/5 coffees",
            document.RootElement
                .GetProperty("storeCard")
                .GetProperty("primaryFields")[0]
                .GetProperty("value")
                .GetString());

        using var conditionalRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/wallet/v1/passes/{PassTypeIdentifier}/{delivery.Card.WalletPassId}");
        conditionalRequest.Headers.Authorization = new AuthenticationHeaderValue("ApplePass", delivery.AuthenticationToken);
        conditionalRequest.Headers.TryAddWithoutValidation("If-Modified-Since", passResponse.Content.Headers.LastModified!.Value.ToString("R", CultureInfo.InvariantCulture));

        var notModifiedResponse = await client.SendAsync(conditionalRequest);

        Assert.Equal(HttpStatusCode.NotModified, notModifiedResponse.StatusCode);
    }

    [Fact]
    public async Task WalletWebService_UnregistersDeviceAndStopsReportingUpdates()
    {
        await using var factory = new WalletPassWebApplicationFactory();
        using var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<DemoPlatformState>();

        var workspace = await state.CreateMerchantAsync(
            "Jo's Coffee",
            "Bristol",
            "BS1 4DJ",
            "owner@joscoffee.test",
            logoUpload: null,
            fallbackLogoUrl: FallbackLogoUrl,
            primaryColor: "#1f3731",
            accentColor: "#f4c15d",
            baseUri: client.BaseAddress!.ToString());
        workspace = state.CreateProgramme(workspace.Merchant.MerchantId, "coffee-visits", client.BaseAddress!.ToString())!;
        var selectedProgramme = Assert.IsType<Fic.Contracts.LoyaltyProgrammeSnapshot>(workspace.SelectedProgramme);

        var card = state.JoinCustomer(selectedProgramme.JoinCode)!;
        var delivery = state.GetWalletPassDelivery(card.CardId)!;
        var baseline = delivery.Card.LastUpdatedUtc.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApplePass", delivery.AuthenticationToken);

        var registerResponse = await client.PostAsync(
            $"/wallet/v1/devices/device-demo-2/registrations/{PassTypeIdentifier}/{delivery.Card.WalletPassId}",
            new StringContent("{\"pushToken\":\"push-token-demo-2\"}", Encoding.UTF8, "application/json"));

        Assert.True(
            registerResponse.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created but got {(int)registerResponse.StatusCode}: {await registerResponse.Content.ReadAsStringAsync()}");

        var deleteResponse = await client.DeleteAsync(
            $"/wallet/v1/devices/device-demo-2/registrations/{PassTypeIdentifier}/{delivery.Card.WalletPassId}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        state.AwardVisit(
            workspace.Merchant.MerchantId,
            selectedProgramme.ProgrammeId,
            delivery.Card.CardCode,
            client.BaseAddress!.ToString());

        var updatesResponse = await client.GetAsync(
            $"/wallet/v1/devices/device-demo-2/registrations/{PassTypeIdentifier}?passesUpdatedSince={baseline}");

        var updates = await updatesResponse.Content.ReadFromJsonAsync<WalletPassSerialUpdatePayload>();

        Assert.NotNull(updates);
        Assert.Empty(updates!.SerialNumbers);
    }

    private sealed class WalletPassWebApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
    {
        private readonly TemporaryDirectory _tempDirectory = new();
        private readonly string _signingPassword = "fic-demo-password";
        private readonly string _signingP12Path;
        private readonly string _wwdrPath;

        public WalletPassWebApplicationFactory()
        {
            _signingP12Path = Path.Combine(_tempDirectory.Path, "fic-demo-signing.p12");
            _wwdrPath = Path.Combine(_tempDirectory.Path, "AppleWWDR.cer");

            using var signingRsa = RSA.Create(2048);
            var signingRequest = new CertificateRequest(
                "CN=FIC Demo Signing",
                signingRsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            signingRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            signingRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
            using var signingCert = signingRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
            File.WriteAllBytes(_signingP12Path, signingCert.Export(X509ContentType.Pfx, _signingPassword));

            using var wwdrRsa = RSA.Create(2048);
            var wwdrRequest = new CertificateRequest(
                "CN=Apple WWDR Test",
                wwdrRsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            wwdrRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            using var wwdrCert = wwdrRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
            File.WriteAllBytes(_wwdrPath, wwdrCert.Export(X509ContentType.Cert));
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Wallet:AppleWalletSigningConfigured"] = "true",
                    ["Wallet:AppleWallet:PassTypeIdentifier"] = PassTypeIdentifier,
                    ["Wallet:AppleWallet:TeamIdentifier"] = "ABCDE12345",
                    ["Wallet:AppleWallet:OrganizationName"] = "FIC Demo",
                    ["Wallet:AppleWallet:Description"] = "Coffee loyalty card",
                    ["Wallet:AppleWallet:P12CertificatePath"] = _signingP12Path,
                    ["Wallet:AppleWallet:P12CertificatePassword"] = _signingPassword,
                    ["Wallet:AppleWallet:WwdrCertificatePath"] = _wwdrPath
                });
            });
        }

        public new async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            _tempDirectory.Dispose();
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"fic-wallet-web-tests-{Guid.NewGuid():N}");
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

    private sealed record WalletPassSerialUpdatePayload(
        string? LastUpdated,
        IReadOnlyList<string> SerialNumbers);
}
