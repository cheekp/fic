using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Fic.Contracts;

namespace Fic.WalletPasses;

public sealed class AppleWalletPassUpdateNotifier(
    AppleWalletPassOptions options,
    HttpMessageHandler? httpMessageHandler = null) : IWalletPassUpdateNotifier
{
    private const string JsonPayload = "{}";

    public WalletPassPushCapability GetCapability()
    {
        var diagnostics = new List<string>();

        if (!options.SigningConfigured)
        {
            diagnostics.Add("Apple Wallet signing is turned off for this environment.");
        }

        if (!options.PushNotificationsEnabled)
        {
            diagnostics.Add("Wallet refresh push delivery is turned off for this environment.");
        }

        if (string.IsNullOrWhiteSpace(options.PassTypeIdentifier))
        {
            diagnostics.Add("Pass type identifier is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.P12CertificatePath))
        {
            diagnostics.Add("Signing certificate path is missing.");
        }
        else if (!File.Exists(options.P12CertificatePath))
        {
            diagnostics.Add("Signing certificate file was not found.");
        }
        else if (!AppleWalletCertificateLoader.TryLoadSigningCertificate(options, out _, out var signingIssue))
        {
            diagnostics.Add(signingIssue!);
        }

        if (diagnostics.Count == 0)
        {
            return new WalletPassPushCapability(
                true,
                "Wallet refresh requests can be sent for registered devices after a stamp or redeem.");
        }

        var helperText = options.PushNotificationsEnabled
            ? "Finish the Wallet refresh setup to request pass updates automatically after merchant actions."
            : "Wallet refresh push delivery is turned off, so pass updates rely on manual refresh behavior.";

        return new WalletPassPushCapability(false, helperText, diagnostics);
    }

    public async Task<WalletPassUpdateDispatchResult> NotifyPassUpdatedAsync(
        WalletCardSnapshot card,
        IReadOnlyList<string> pushTokens,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var capability = GetCapability();
        if (!capability.IsReady)
        {
            return new WalletPassUpdateDispatchResult(
                pushTokens.Count,
                0,
                0,
                true,
                "Wallet refresh was not requested because push delivery is not configured.",
                capability.DiagnosticItems);
        }

        if (pushTokens.Count == 0)
        {
            return new WalletPassUpdateDispatchResult(
                0,
                0,
                0,
                true,
                "Wallet refresh will start once a device has registered this pass for updates.");
        }

        var distinctTokens = pushTokens
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Select(token => token.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (distinctTokens.Length == 0)
        {
            return new WalletPassUpdateDispatchResult(
                0,
                0,
                0,
                true,
                "Wallet refresh will start once a device has registered this pass for updates.");
        }

        using var client = CreateClient();
        var successCount = 0;
        var diagnostics = new List<string>();

        foreach (var pushToken in distinctTokens)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildDeviceEndpoint(pushToken))
            {
                Version = HttpVersion.Version20,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
                Content = new StringContent(JsonPayload, Encoding.UTF8, "application/json")
            };

            request.Headers.TryAddWithoutValidation("apns-topic", options.PassTypeIdentifier);

            try
            {
                using var response = await client.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    successCount++;
                    continue;
                }

                diagnostics.Add($"APNs rejected push for token {pushToken} with {(int)response.StatusCode} {response.StatusCode}.");
            }
            catch (HttpRequestException ex)
            {
                diagnostics.Add($"APNs request failed for token {pushToken}: {ex.Message}");
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                diagnostics.Add($"APNs request timed out for token {pushToken}.");
            }
        }

        var failureCount = distinctTokens.Length - successCount;
        var summary = successCount > 0
            ? $"Wallet refresh requested for {successCount} registered device{(successCount == 1 ? string.Empty : "s")}."
            : "Wallet refresh request did not reach any registered devices.";

        return new WalletPassUpdateDispatchResult(
            distinctTokens.Length,
            successCount,
            failureCount,
            false,
            summary,
            diagnostics.Count == 0 ? null : diagnostics);
    }

    private HttpClient CreateClient()
    {
        if (httpMessageHandler is not null)
        {
            return new HttpClient(httpMessageHandler, disposeHandler: false);
        }

        var signingCertificate = AppleWalletCertificateLoader.LoadSigningCertificate(options);
        var handler = BuildHandler(signingCertificate);
        return new HttpClient(handler, disposeHandler: true);
    }

    private HttpMessageHandler BuildHandler(X509Certificate2 signingCertificate)
    {
        var socketsHandler = new SocketsHttpHandler();
        socketsHandler.SslOptions.ClientCertificates = new X509CertificateCollection { signingCertificate };
        return socketsHandler;
    }

    private Uri BuildDeviceEndpoint(string pushToken)
    {
        if (!string.IsNullOrWhiteSpace(options.PushEndpointOverride))
        {
            return new Uri(new Uri(options.PushEndpointOverride, UriKind.Absolute), $"3/device/{pushToken}");
        }

        var endpoint = options.UseSandboxPushEndpoint
            ? "https://api.sandbox.push.apple.com/"
            : "https://api.push.apple.com/";

        return new Uri(new Uri(endpoint, UriKind.Absolute), $"3/device/{pushToken}");
    }
}
