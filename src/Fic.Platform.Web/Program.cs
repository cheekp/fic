using Fic.Platform.Web.Components;
using Fic.Platform.Web.Services;
using Fic.WalletPasses;
using Fic.MerchantAccounts;
using Microsoft.Extensions.FileProviders;
using Azure.Storage.Blobs;
using System.Globalization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseStaticWebAssets();
}

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var useBlobBrandAssets = builder.Configuration.GetValue("Features:UseBlobBrandAssets", false);
var brandAssetsConnectionString = builder.Configuration.GetConnectionString("brandassets");

if (useBlobBrandAssets && !string.IsNullOrWhiteSpace(brandAssetsConnectionString))
{
    builder.Services.AddSingleton(new BlobServiceClient(brandAssetsConnectionString));
    builder.Services.AddSingleton<IMerchantBrandAssetStore, BlobMerchantBrandAssetStore>();
}
else
{
    builder.Services.AddSingleton<IMerchantBrandAssetStore, LocalMerchantBrandAssetStore>();
}

builder.Services.AddSingleton<DemoPlatformState>();
builder.Services.AddSingleton<JoinQrCodeService>();
builder.Services.AddSingleton<MerchantBrandPresentationService>();
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    var options = new AppleWalletPassOptions();

    configuration.GetSection("Wallet:AppleWallet").Bind(options);
    options.SigningConfigured = configuration.GetValue("Wallet:AppleWalletSigningConfigured", false);
    options.DefaultAssetDirectory = Path.Combine(
        environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
        "wallet-assets");

    return options;
});
builder.Services.AddSingleton<IAppleWalletPassService>(sp =>
{
    return new AppleWalletPassService(
        sp.GetRequiredService<AppleWalletPassOptions>(),
        sp.GetRequiredService<IMerchantBrandAssetStore>());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/wallet/v1", StringComparison.OrdinalIgnoreCase),
    branch => branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));

app.Map("/wallet/v1", walletApp =>
{
    walletApp.Run(async context =>
    {
        try
        {
            var segments = context.Request.Path.Value?
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? [];
            var requestServices = context.RequestServices;
            var platformState = requestServices.GetRequiredService<DemoPlatformState>();
            var options = requestServices.GetRequiredService<AppleWalletPassOptions>();

            if (segments.Length == 5
                && string.Equals(segments[0], "devices", StringComparison.OrdinalIgnoreCase)
                && string.Equals(segments[2], "registrations", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsPost(context.Request.Method))
            {
                var deviceLibraryIdentifier = segments[1];
                var passTypeIdentifier = segments[3];
                var serialNumber = segments[4];

                if (!MatchesWalletPassType(options, passTypeIdentifier))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                var registration = await ReadRequestJsonAsync<WalletPassRegistrationRequest>(context.Request, context.RequestAborted);
                if (registration is null || string.IsNullOrWhiteSpace(registration.PushToken))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                var authenticationToken = TryReadApplePassToken(context.Request);
                if (string.IsNullOrWhiteSpace(authenticationToken))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var result = platformState.RegisterWalletPassDevice(
                    deviceLibraryIdentifier,
                    serialNumber,
                    authenticationToken,
                    registration.PushToken);

                context.Response.StatusCode = result.Status switch
                {
                    WalletPassRegistrationStatus.Created => StatusCodes.Status201Created,
                    WalletPassRegistrationStatus.Updated => StatusCodes.Status200OK,
                    WalletPassRegistrationStatus.NotFound => StatusCodes.Status404NotFound,
                    WalletPassRegistrationStatus.Unauthorized => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status400BadRequest
                };

                return;
            }

            if (segments.Length == 5
                && string.Equals(segments[0], "devices", StringComparison.OrdinalIgnoreCase)
                && string.Equals(segments[2], "registrations", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsDelete(context.Request.Method))
            {
                var deviceLibraryIdentifier = segments[1];
                var passTypeIdentifier = segments[3];
                var serialNumber = segments[4];

                if (!MatchesWalletPassType(options, passTypeIdentifier))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                var authenticationToken = TryReadApplePassToken(context.Request);
                if (string.IsNullOrWhiteSpace(authenticationToken))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var result = platformState.RemoveWalletPassDeviceRegistration(
                    deviceLibraryIdentifier,
                    serialNumber,
                    authenticationToken);

                context.Response.StatusCode = result.Status switch
                {
                    WalletPassRegistrationStatus.Updated => StatusCodes.Status200OK,
                    WalletPassRegistrationStatus.NotFound => StatusCodes.Status404NotFound,
                    WalletPassRegistrationStatus.Unauthorized => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status400BadRequest
                };

                return;
            }

            if (segments.Length == 4
                && string.Equals(segments[0], "devices", StringComparison.OrdinalIgnoreCase)
                && string.Equals(segments[2], "registrations", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsGet(context.Request.Method))
            {
                var deviceLibraryIdentifier = segments[1];
                var passTypeIdentifier = segments[3];

                if (!MatchesWalletPassType(options, passTypeIdentifier))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                var updatedSince = ParseWalletUpdateTag(context.Request.Query["passesUpdatedSince"]);
                var updates = platformState.GetUpdatedWalletPassSerialNumbers(deviceLibraryIdentifier, updatedSince);
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsJsonAsync(
                    new WalletPassSerialUpdateResponse(updates.LastUpdatedTag, updates.SerialNumbers),
                    context.RequestAborted);
                return;
            }

            if (segments.Length == 3
                && string.Equals(segments[0], "passes", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsGet(context.Request.Method))
            {
                var passTypeIdentifier = segments[1];
                var serialNumber = segments[2];

                if (!MatchesWalletPassType(options, passTypeIdentifier))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                var authenticationToken = TryReadApplePassToken(context.Request);
                if (string.IsNullOrWhiteSpace(authenticationToken))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var delivery = platformState.GetWalletPassDeliveryBySerialNumber(serialNumber);
                if (delivery is null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                if (!string.Equals(delivery.AuthenticationToken, authenticationToken, StringComparison.Ordinal))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var lastModifiedUtc = ToHttpDatePrecision(delivery.Card.LastUpdatedUtc);
                if (TryParseIfModifiedSince(context.Request.Headers.IfModifiedSince, out var modifiedSince)
                    && lastModifiedUtc <= modifiedSince)
                {
                    context.Response.StatusCode = StatusCodes.Status304NotModified;
                    return;
                }

                var walletPasses = requestServices.GetRequiredService<IAppleWalletPassService>();
                var package = await walletPasses.CreatePackageAsync(
                    delivery.Card,
                    delivery.AuthenticationToken,
                    BuildWalletWebServiceUrl(context.Request),
                    context.RequestAborted);

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = package.ContentType;
                context.Response.Headers.ContentDisposition = $"attachment; filename=\"{package.FileName}\"";
                context.Response.Headers.LastModified = lastModifiedUtc.ToString("R", CultureInfo.InvariantCulture);
                await context.Response.Body.WriteAsync(package.Bytes, context.RequestAborted);
                return;
            }

            if (segments.Length == 1
                && string.Equals(segments[0], "log", StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsPost(context.Request.Method))
            {
                var logger = requestServices.GetRequiredService<ILogger<Program>>();
                var logRequest = await ReadRequestJsonAsync<WalletPassLogRequest>(context.Request, context.RequestAborted);
                if (logRequest?.Logs is { Count: > 0 })
                {
                    foreach (var entry in logRequest.Logs)
                    {
                        logger.LogInformation("wallet_pass_log {Entry}", entry);
                    }
                }

                context.Response.StatusCode = StatusCodes.Status200OK;
                return;
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "wallet_web_service_error method={Method} path={Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    });
});

app.UseAntiforgery();

app.MapGet("/merchant-brand-assets/{**assetPath}", async (
    string assetPath,
    IMerchantBrandAssetStore brandAssetStore,
    CancellationToken cancellationToken) =>
{
    var asset = await brandAssetStore.GetAssetAsync(
        $"{MerchantBrandAssetDefaults.PublicRequestPath}/{assetPath}",
        cancellationToken);

    return asset is null
        ? Results.NotFound()
        : Results.File(asset.Bytes, asset.ContentType);
});

app.MapGet("/wallet/passes/{cardId:guid}.pkpass", async (
    Guid cardId,
    HttpRequest request,
    DemoPlatformState platformState,
    IAppleWalletPassService walletPasses,
    CancellationToken cancellationToken) =>
{
    var delivery = platformState.GetWalletPassDelivery(cardId);
    if (delivery is null)
    {
        return Results.NotFound();
    }

    if (!walletPasses.GetCapability().SupportsAppleWalletPass)
    {
        return Results.Redirect($"/wallet/card/{cardId}");
    }

    var package = await walletPasses.CreatePackageAsync(
        delivery.Card,
        delivery.AuthenticationToken,
        BuildWalletWebServiceUrl(request),
        cancellationToken);
    return Results.File(package.Bytes, package.ContentType, package.FileName);
});

app.MapStaticAssets();
app.MapDefaultEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static bool MatchesWalletPassType(AppleWalletPassOptions options, string passTypeIdentifier) =>
    string.Equals(options.PassTypeIdentifier, passTypeIdentifier, StringComparison.Ordinal);

static string BuildWalletWebServiceUrl(HttpRequest request) =>
    new Uri(
        new Uri($"{request.Scheme}://{request.Host}/", UriKind.Absolute),
        "wallet/v1/")
    .ToString();

static string? TryReadApplePassToken(HttpRequest request)
{
    var authorization = request.Headers.Authorization.ToString();
    const string prefix = "ApplePass ";
    return authorization.StartsWith(prefix, StringComparison.Ordinal)
        ? authorization[prefix.Length..].Trim()
        : null;
}

static DateTimeOffset? ParseWalletUpdateTag(string? updateTag)
{
    if (string.IsNullOrWhiteSpace(updateTag))
    {
        return null;
    }

    return long.TryParse(updateTag, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixMilliseconds)
        ? DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds)
        : null;
}

static bool TryParseIfModifiedSince(string? ifModifiedSince, out DateTimeOffset value) =>
    DateTimeOffset.TryParse(ifModifiedSince, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out value);

static DateTimeOffset ToHttpDatePrecision(DateTimeOffset value) =>
    new(value.UtcDateTime.Year, value.UtcDateTime.Month, value.UtcDateTime.Day, value.UtcDateTime.Hour, value.UtcDateTime.Minute, value.UtcDateTime.Second, TimeSpan.Zero);

static async Task<T?> ReadRequestJsonAsync<T>(HttpRequest request, CancellationToken cancellationToken = default)
{
    using var reader = new StreamReader(request.Body, leaveOpen: true);
    var json = await reader.ReadToEndAsync(cancellationToken);

    return string.IsNullOrWhiteSpace(json)
        ? default
        : JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
}

public sealed record WalletPassRegistrationRequest(string PushToken);

public sealed record WalletPassSerialUpdateResponse(string? LastUpdated, IReadOnlyList<string> SerialNumbers);

public sealed record WalletPassLogRequest(IReadOnlyList<string>? Logs);

public partial class Program;
