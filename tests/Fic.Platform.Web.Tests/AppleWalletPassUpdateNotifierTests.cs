using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fic.Contracts;
using Fic.WalletPasses;
using Xunit;

namespace Fic.Platform.Web.Tests;

public sealed class AppleWalletPassUpdateNotifierTests
{
    [Fact]
    public void GetCapability_ReturnsNotReady_WhenPushIsDisabled()
    {
        var notifier = new AppleWalletPassUpdateNotifier(new AppleWalletPassOptions
        {
            SigningConfigured = true,
            PushNotificationsEnabled = false,
            PassTypeIdentifier = "pass.com.fic.demo"
        });

        var capability = notifier.GetCapability();

        Assert.False(capability.IsReady);
        Assert.Contains("turned off", capability.HelperText, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(capability.DiagnosticItems);
    }

    [Fact]
    public async Task NotifyPassUpdatedAsync_SendsTopicToConfiguredEndpoint()
    {
        using var tempDirectory = new TemporaryDirectory();
        var p12Path = CreateSigningCertificate(tempDirectory.Path, out var password);
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var notifier = new AppleWalletPassUpdateNotifier(
            new AppleWalletPassOptions
            {
                SigningConfigured = true,
                PushNotificationsEnabled = true,
                PassTypeIdentifier = "pass.com.fic.demo",
                P12CertificatePath = p12Path,
                P12CertificatePassword = password,
                PushEndpointOverride = "https://push.example.test/"
            },
            handler);

        var result = await notifier.NotifyPassUpdatedAsync(
            CreateWalletCard(),
            ["token-one", "token-two", "token-one"]);

        Assert.Equal(2, result.RegistrationCount);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.False(result.Skipped);
        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.StartsWith("https://push.example.test/3/device/", request.RequestUri!.ToString(), StringComparison.Ordinal);
            Assert.Equal("pass.com.fic.demo", request.Headers.GetValues("apns-topic").Single());
        });
    }

    [Fact]
    public async Task NotifyPassUpdatedAsync_Skips_WhenNoRegisteredDevicesExist()
    {
        using var tempDirectory = new TemporaryDirectory();
        var p12Path = CreateSigningCertificate(tempDirectory.Path, out var password);
        var notifier = new AppleWalletPassUpdateNotifier(
            new AppleWalletPassOptions
            {
                SigningConfigured = true,
                PushNotificationsEnabled = true,
                PassTypeIdentifier = "pass.com.fic.demo",
                P12CertificatePath = p12Path,
                P12CertificatePassword = password
            },
            new RecordingHandler(HttpStatusCode.OK));

        var result = await notifier.NotifyPassUpdatedAsync(CreateWalletCard(), []);

        Assert.True(result.Skipped);
        Assert.Equal(0, result.RegistrationCount);
        Assert.Contains("registered this pass", result.Summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task NotifyPassUpdatedAsync_ReturnsUnavailable_WhenPushIsNotConfigured()
    {
        var notifier = new AppleWalletPassUpdateNotifier(new AppleWalletPassOptions
        {
            SigningConfigured = true,
            PushNotificationsEnabled = false,
            PassTypeIdentifier = "pass.com.fic.demo"
        });

        var result = await notifier.NotifyPassUpdatedAsync(CreateWalletCard(), ["push-token-one"]);

        Assert.True(result.Skipped);
        Assert.Equal(1, result.RegistrationCount);
        Assert.Contains("unavailable", result.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, result.RetryableFailureCount);
        Assert.Equal(0, result.PermanentFailureCount);
    }

    [Fact]
    public async Task NotifyPassUpdatedAsync_ClassifiesRetryableAndPermanentTokenFailures()
    {
        using var tempDirectory = new TemporaryDirectory();
        var p12Path = CreateSigningCertificate(tempDirectory.Path, out var password);
        var handler = new RecordingHandler((request, _) =>
        {
            var token = request.RequestUri!.Segments[^1];
            return token switch
            {
                "token-success" => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)),
                "token-retry" => Task.FromResult(CreateApnsError(HttpStatusCode.ServiceUnavailable, "ServiceUnavailable")),
                "token-invalid" => Task.FromResult(CreateApnsError(HttpStatusCode.Gone, "Unregistered")),
                _ => Task.FromResult(CreateApnsError(HttpStatusCode.BadRequest, "BadDeviceToken"))
            };
        });
        var notifier = new AppleWalletPassUpdateNotifier(
            new AppleWalletPassOptions
            {
                SigningConfigured = true,
                PushNotificationsEnabled = true,
                PassTypeIdentifier = "pass.com.fic.demo",
                P12CertificatePath = p12Path,
                P12CertificatePassword = password,
                PushEndpointOverride = "https://push.example.test/"
            },
            handler);

        var result = await notifier.NotifyPassUpdatedAsync(
            CreateWalletCard(),
            ["token-success", "token-retry", "token-invalid"]);

        Assert.Equal(3, result.RegistrationCount);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(2, result.FailureCount);
        Assert.Equal(1, result.RetryableFailureCount);
        Assert.Equal(1, result.PermanentFailureCount);
        Assert.True(result.HasInvalidatedPushTokens);
        Assert.Equal(["token-invalid"], result.InvalidatedPushTokens);
        Assert.Contains("retry needed", result.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no longer valid", result.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(result.DiagnosticItems);
        Assert.Contains(result.DiagnosticItems!, message => message.Contains("retryable", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.DiagnosticItems!, message => message.Contains("permanent token", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task NotifyPassUpdatedAsync_TreatsBadDeviceTokenAsPermanentTokenFailure()
    {
        using var tempDirectory = new TemporaryDirectory();
        var p12Path = CreateSigningCertificate(tempDirectory.Path, out var password);
        var handler = new RecordingHandler((_, _) =>
            Task.FromResult(CreateApnsError(HttpStatusCode.BadRequest, "BadDeviceToken")));
        var notifier = new AppleWalletPassUpdateNotifier(
            new AppleWalletPassOptions
            {
                SigningConfigured = true,
                PushNotificationsEnabled = true,
                PassTypeIdentifier = "pass.com.fic.demo",
                P12CertificatePath = p12Path,
                P12CertificatePassword = password,
                PushEndpointOverride = "https://push.example.test/"
            },
            handler);

        var result = await notifier.NotifyPassUpdatedAsync(CreateWalletCard(), ["token-bad-device"]);

        Assert.Equal(1, result.PermanentFailureCount);
        Assert.Equal(["token-bad-device"], result.InvalidatedPushTokens);
        Assert.Contains("no longer valid", result.Summary, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateSigningCertificate(string directory, out string password)
    {
        password = "fic-demo-password";
        var p12Path = Path.Combine(directory, "fic-demo-signing.p12");

        using var signingRsa = RSA.Create(2048);
        var signingRequest = new CertificateRequest(
            "CN=FIC Demo Signing",
            signingRsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        signingRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        signingRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        using var signingCert = signingRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
        File.WriteAllBytes(p12Path, signingCert.Export(X509ContentType.Pfx, password));
        return p12Path;
    }

    private static WalletCardSnapshot CreateWalletCard() =>
        new(
            Guid.Parse("c03f6e57-3fa7-4a3d-a661-7105619a8ae6"),
            Guid.Parse("7e4f9f3b-6298-4665-93c5-ef3721cb9ab7"),
            Guid.Parse("9325f2fb-3bb9-49b7-a0b8-948852cf00cc"),
            "card-123",
            "wallet-pass-123",
            "Jo's Coffee",
            "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Y1koXUAAAAASUVORK5CYII=",
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

    private static HttpResponseMessage CreateApnsError(HttpStatusCode statusCode, string reason)
    {
        var response = new HttpResponseMessage(statusCode);
        response.Content = new StringContent($"{{\"reason\":\"{reason}\"}}", System.Text.Encoding.UTF8, "application/json");
        return response;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responseFactory;

        public RecordingHandler(HttpStatusCode statusCode)
            : this((_, _) => Task.FromResult(new HttpResponseMessage(statusCode)))
        {
        }

        public RecordingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(CloneRequest(request));
            return await _responseFactory(request, cancellationToken);
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version,
                VersionPolicy = request.VersionPolicy
            };

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"fic-push-tests-{Guid.NewGuid():N}");
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
