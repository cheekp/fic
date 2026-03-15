using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Fic.Contracts;

namespace Fic.WalletPasses;

public sealed class AppleWalletPassUpdateNotifier(
    AppleWalletPassOptions options,
    HttpMessageHandler? httpMessageHandler = null) : IWalletPassUpdateNotifier
{
    private const string JsonPayload = "{}";
    private static readonly string[] InvalidTokenReasons =
    [
        "BadDeviceToken",
        "DeviceTokenNotForTopic",
        "Unregistered"
    ];

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
                "Wallet refresh unavailable because push delivery is not configured.",
                capability.DiagnosticItems);
        }

        if (pushTokens.Count == 0)
        {
            return new WalletPassUpdateDispatchResult(
                0,
                0,
                0,
                true,
                "Wallet refresh skipped until a device has registered this pass for updates.");
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
                "Wallet refresh skipped until a device has registered this pass for updates.");
        }

        using var client = CreateClient();
        var successCount = 0;
        var retryableFailureCount = 0;
        var permanentFailureCount = 0;
        var invalidatedTokens = new List<string>();
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

                var reason = await ReadApnsReasonAsync(response, cancellationToken);
                var classification = ClassifyFailure(response.StatusCode, reason);

                switch (classification)
                {
                    case DispatchFailureClassification.Retryable:
                        retryableFailureCount++;
                        diagnostics.Add(BuildRetryableDiagnostic(pushToken, response.StatusCode, reason));
                        break;
                    case DispatchFailureClassification.PermanentToken:
                        permanentFailureCount++;
                        invalidatedTokens.Add(pushToken);
                        diagnostics.Add(BuildPermanentTokenDiagnostic(pushToken, response.StatusCode, reason));
                        break;
                    default:
                        permanentFailureCount++;
                        diagnostics.Add(BuildPermanentDiagnostic(pushToken, response.StatusCode, reason));
                        break;
                }
            }
            catch (HttpRequestException ex)
            {
                retryableFailureCount++;
                diagnostics.Add($"APNs retryable failure for token {pushToken}: {ex.Message}");
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                retryableFailureCount++;
                diagnostics.Add($"APNs retryable failure for token {pushToken}: request timed out.");
            }
        }

        var failureCount = retryableFailureCount + permanentFailureCount;
        var summary = BuildSummary(successCount, retryableFailureCount, permanentFailureCount);
        var invalidatedDistinct = invalidatedTokens
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new WalletPassUpdateDispatchResult(
            distinctTokens.Length,
            successCount,
            failureCount,
            false,
            summary,
            diagnostics.Count == 0 ? null : diagnostics,
            retryableFailureCount,
            permanentFailureCount,
            invalidatedDistinct.Length == 0 ? null : invalidatedDistinct);
    }

    private static DispatchFailureClassification ClassifyFailure(HttpStatusCode statusCode, string? reason)
    {
        if (IsRetryableStatusCode(statusCode))
        {
            return DispatchFailureClassification.Retryable;
        }

        if (IsPermanentTokenFailure(statusCode, reason))
        {
            return DispatchFailureClassification.PermanentToken;
        }

        return (int)statusCode >= 500
            ? DispatchFailureClassification.Retryable
            : DispatchFailureClassification.Permanent;
    }

    private static bool IsRetryableStatusCode(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static bool IsPermanentTokenFailure(HttpStatusCode statusCode, string? reason)
    {
        if (statusCode == HttpStatusCode.Gone)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return false;
        }

        return InvalidTokenReasons.Any(tokenReason => string.Equals(tokenReason, reason, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<string?> ReadApnsReasonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return null;
        }

        try
        {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            using var document = JsonDocument.Parse(payload);
            return document.RootElement.TryGetProperty("reason", out var reasonElement)
                ? reasonElement.GetString()
                : null;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildSummary(int successCount, int retryableFailureCount, int permanentFailureCount)
    {
        if (successCount > 0 && retryableFailureCount == 0 && permanentFailureCount == 0)
        {
            return $"Wallet refresh sent to {successCount} registered device{(successCount == 1 ? string.Empty : "s")}.";
        }

        if (successCount == 0 && retryableFailureCount > 0 && permanentFailureCount == 0)
        {
            return "Wallet refresh retry needed because APNs did not confirm any device updates.";
        }

        if (successCount == 0 && retryableFailureCount == 0 && permanentFailureCount > 0)
        {
            return "Wallet refresh unavailable because registered devices were no longer valid.";
        }

        if (successCount > 0 && retryableFailureCount > 0 && permanentFailureCount == 0)
        {
            return $"Wallet refresh sent to {successCount} device{(successCount == 1 ? string.Empty : "s")}; retry needed for {retryableFailureCount} device{(retryableFailureCount == 1 ? string.Empty : "s")}.";
        }

        if (successCount > 0 && retryableFailureCount == 0 && permanentFailureCount > 0)
        {
            return $"Wallet refresh sent to {successCount} device{(successCount == 1 ? string.Empty : "s")}; some registered devices were no longer valid.";
        }

        if (successCount == 0 && retryableFailureCount > 0 && permanentFailureCount > 0)
        {
            return "Wallet refresh retry needed and some registered devices were no longer valid.";
        }

        return $"Wallet refresh sent to {successCount} device{(successCount == 1 ? string.Empty : "s")}; retry needed for {retryableFailureCount} and some registered devices were no longer valid.";
    }

    private static string BuildRetryableDiagnostic(string pushToken, HttpStatusCode statusCode, string? reason) =>
        BuildDiagnostic("APNs retryable failure", pushToken, statusCode, reason);

    private static string BuildPermanentDiagnostic(string pushToken, HttpStatusCode statusCode, string? reason) =>
        BuildDiagnostic("APNs permanent failure", pushToken, statusCode, reason);

    private static string BuildPermanentTokenDiagnostic(string pushToken, HttpStatusCode statusCode, string? reason) =>
        BuildDiagnostic("APNs permanent token failure", pushToken, statusCode, reason);

    private static string BuildDiagnostic(string prefix, string pushToken, HttpStatusCode statusCode, string? reason)
    {
        var reasonSuffix = string.IsNullOrWhiteSpace(reason) ? string.Empty : $" ({reason})";
        return $"{prefix} for token {pushToken} with {(int)statusCode} {statusCode}{reasonSuffix}.";
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

    private enum DispatchFailureClassification
    {
        Retryable,
        Permanent,
        PermanentToken
    }
}
