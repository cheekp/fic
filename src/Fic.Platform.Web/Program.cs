using Fic.Platform.Web.Components;
using Fic.Platform.Web.Services;
using Fic.WalletPasses;
using Fic.MerchantAccounts;
using Fic.Contracts;
using Microsoft.Extensions.FileProviders;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Globalization;
using System.Text.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseStaticWebAssets();
}

builder.AddServiceDefaults();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "fic.merchant.auth";
        options.LoginPath = "/account/login";
        options.AccessDeniedPath = "/account/access-denied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("fic-next-dev", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            && (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(uri.Host, "::1", StringComparison.OrdinalIgnoreCase)))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddCascadingAuthenticationState();
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

builder.Services.AddSingleton<IProgrammeCatalogueRepository, JsonProgrammeCatalogueRepository>();
builder.Services.AddSingleton<DemoPlatformState>();
builder.Services.AddSingleton<PortalNavigationContractBuilder>();
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
builder.Services.AddSingleton<IWalletPassUpdateNotifier>(sp =>
    new AppleWalletPassUpdateNotifier(sp.GetRequiredService<AppleWalletPassOptions>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseCors("fic-next-dev");
app.UseAuthentication();
app.UseWhen(
    context =>
        !context.Request.Path.StartsWithSegments("/wallet/v1", StringComparison.OrdinalIgnoreCase)
        && !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
    branch => branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/portal/merchant", StringComparison.OrdinalIgnoreCase),
    branch => branch.Use(async (context, next) =>
    {
        if (!TryGetMerchantRouteId(context.Request.Path, out var merchantId))
        {
            await next();
            return;
        }

        if (!(context.User.Identity?.IsAuthenticated ?? false))
        {
            context.Response.Redirect(BuildLoginRedirectUrl(context.Request));
            return;
        }

        var signedInMerchantId = MerchantSessionClaims.TryGetMerchantId(context.User);
        if (!signedInMerchantId.HasValue || signedInMerchantId.Value != merchantId)
        {
            context.Response.Redirect($"/account/access-denied?merchantId={merchantId:D}");
            return;
        }

        await next();
    }));
app.UseAuthorization();

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

app.MapPost("/account/session/login", async (
    HttpContext context,
    DemoPlatformState platformState) =>
{
    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var email = form["email"].ToString().Trim();
    var password = form["password"].ToString();
    var returnUrl = NormalizeLocalReturnUrl(form["returnUrl"].ToString());

    var result = platformState.AuthenticateMerchant(email, password);
    if (result.Status != MerchantAuthenticationStatus.Authenticated || result.Merchant is null)
    {
        var error = result.Status switch
        {
            MerchantAuthenticationStatus.CredentialsNotConfigured => "credentials-not-configured",
            MerchantAuthenticationStatus.NotFound => "merchant-not-found",
            _ => "invalid-credentials"
        };

        return Results.LocalRedirect(BuildLoginUrl(returnUrl, error, email));
    }

    await SignInMerchantAsync(context, result.Merchant);
    return Results.LocalRedirect(returnUrl ?? $"/portal/merchant/{result.Merchant.MerchantId}");
}).DisableAntiforgery();

app.MapPost("/account/session/complete-signup", async (
    HttpContext context,
    DemoPlatformState platformState) =>
{
    var form = await context.Request.ReadFormAsync(context.RequestAborted);
    var merchantIdValue = form["merchantId"].ToString();
    var selectedPlan = form["plan"].ToString().Trim();
    var password = form["password"].ToString();
    var confirmPassword = form["confirmPassword"].ToString();
    var billingReturnUrl = NormalizeLocalReturnUrl(form["returnUrl"].ToString());

    if (!Guid.TryParse(merchantIdValue, out var merchantId))
    {
        return Results.LocalRedirect("/portal/signup");
    }

    if (!string.Equals(selectedPlan, "starter", StringComparison.OrdinalIgnoreCase))
    {
        return Results.LocalRedirect($"/portal/signup/plan/{merchantId:D}?error=unsupported-plan");
    }

    if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
    {
        return Results.LocalRedirect($"/portal/signup/billing/{merchantId:D}?error=password-mismatch&plan=starter");
    }

    var result = platformState.ConfigureMerchantAccess(merchantId, password);
    if (result.Status != MerchantCredentialConfigurationStatus.Updated || result.Merchant is null)
    {
        var error = result.Status switch
        {
            MerchantCredentialConfigurationStatus.NotFound => "merchant-not-found",
            MerchantCredentialConfigurationStatus.AlreadyConfigured => "credentials-already-configured",
            _ => "invalid-password"
        };

        return Results.LocalRedirect($"/portal/signup/billing/{merchantId:D}?error={error}&plan=starter");
    }

    await SignInMerchantAsync(context, result.Merchant);
    return Results.LocalRedirect(billingReturnUrl ?? $"/portal/merchant/{merchantId:D}?tab=programmes&section=programmes&programmeSection=create");
}).DisableAntiforgery();

app.MapGet("/account/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/");
});

var api = app.MapGroup("/api/v1");

api.MapGet("/catalogue/shop-types", (DemoPlatformState platformState) =>
    Results.Ok(platformState.GetShopTypes()));

api.MapGet("/catalogue/card-types", (DemoPlatformState platformState) =>
    Results.Ok(platformState.GetCardTypes()));

api.MapGet("/catalogue/templates", (
    string? shopTypeKey,
    string? cardTypeKey,
    DemoPlatformState platformState) =>
    Results.Ok(platformState.GetProgrammeTemplates(shopTypeKey, cardTypeKey)));

api.MapPost("/merchants", async (
    CreateMerchantApiRequest request,
    HttpRequest httpRequest,
    DemoPlatformState platformState,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.DisplayName)
        || string.IsNullOrWhiteSpace(request.ContactEmail))
    {
        return Results.BadRequest(new ApiErrorResponse("missing-required-fields", "DisplayName and ContactEmail are required."));
    }

    var workspace = await platformState.CreateMerchantAsync(
        request.DisplayName,
        request.TownOrCity?.Trim() ?? string.Empty,
        request.Postcode?.Trim() ?? string.Empty,
        request.ContactEmail,
        logoUpload: null,
        fallbackLogoUrl: BuildFallbackLogo(
            request.DisplayName,
            request.PrimaryColor ?? "#1f3731",
            request.AccentColor ?? "#f4c15d"),
        primaryColor: request.PrimaryColor ?? "#1f3731",
        accentColor: request.AccentColor ?? "#f4c15d",
        baseUri: BuildBaseUri(httpRequest),
        createStarterProgramme: false,
        shopTypeKey: request.ShopTypeKey,
        cancellationToken: cancellationToken);

    return Results.Ok(workspace);
});

api.MapGet("/merchants", (DemoPlatformState platformState) =>
    Results.Ok(platformState.GetMerchantSummaries()));

api.MapGet("/merchants/{merchantId:guid}/workspace", (
    Guid merchantId,
    Guid? programmeId,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var workspace = platformState.GetMerchantWorkspace(
        merchantId,
        BuildBaseUri(httpRequest),
        programmeId);

    return workspace is null
        ? Results.NotFound()
        : Results.Ok(workspace);
});

api.MapGet("/portal/navigation/signup", (
    Guid? merchantId,
    string? step,
    HttpRequest httpRequest,
    DemoPlatformState platformState,
    PortalNavigationContractBuilder builder) =>
{
    var workspace = merchantId.HasValue
        ? platformState.GetMerchantWorkspace(merchantId.Value, BuildBaseUri(httpRequest))
        : null;

    return Results.Ok(builder.BuildSignupNavigation(step, merchantId, workspace));
});

api.MapGet("/merchants/{merchantId:guid}/portal/navigation", (
    Guid merchantId,
    Guid? programmeId,
    string? step,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState,
    PortalNavigationContractBuilder builder) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var workspace = platformState.GetMerchantWorkspace(
        merchantId,
        BuildBaseUri(httpRequest),
        programmeId);

    return workspace is null
        ? Results.NotFound()
        : Results.Ok(builder.BuildWorkspaceNavigation(step, merchantId, workspace, programmeId));
});

api.MapPut("/merchants/{merchantId:guid}/brand", async (
    Guid merchantId,
    UpdateBrandApiRequest request,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState,
    CancellationToken cancellationToken) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var update = await platformState.UpdateMerchantBrandAsync(
        merchantId,
        request.DisplayName,
        request.TownOrCity,
        request.Postcode,
        request.ContactEmail,
        shopTypeKey: request.ShopTypeKey,
        logoUpload: null,
        primaryColor: request.PrimaryColor,
        accentColor: request.AccentColor,
        selectedProgrammeId: request.SelectedProgrammeId,
        baseUri: BuildBaseUri(httpRequest),
        cancellationToken: cancellationToken);

    return update is null ? Results.NotFound() : Results.Ok(update);
});

api.MapPost("/merchants/{merchantId:guid}/brand/logo", async (
    Guid merchantId,
    Guid? selectedProgrammeId,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState,
    CancellationToken cancellationToken) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var form = await httpRequest.ReadFormAsync(cancellationToken);
    var logo = form.Files["logo"];
    if (logo is null || logo.Length == 0)
    {
        return Results.BadRequest(new ApiErrorResponse("missing-logo", "A PNG logo file is required."));
    }

    await using var logoStream = logo.OpenReadStream();
    using var memory = new MemoryStream();
    await logoStream.CopyToAsync(memory, cancellationToken);
    var logoUpload = new MerchantLogoUpload(
        memory.ToArray(),
        string.IsNullOrWhiteSpace(logo.ContentType) ? "image/png" : logo.ContentType,
        string.IsNullOrWhiteSpace(logo.FileName) ? "logo.png" : logo.FileName);

    var currentWorkspace = platformState.GetMerchantWorkspace(
        merchantId,
        BuildBaseUri(httpRequest),
        selectedProgrammeId);

    if (currentWorkspace is null)
    {
        return Results.NotFound();
    }

    var update = await platformState.UpdateMerchantBrandAsync(
        merchantId,
        currentWorkspace.Merchant.DisplayName,
        currentWorkspace.Merchant.TownOrCity,
        currentWorkspace.Merchant.Postcode,
        currentWorkspace.Merchant.ContactEmail,
        shopTypeKey: currentWorkspace.Merchant.ShopTypeKey,
        logoUpload: logoUpload,
        primaryColor: currentWorkspace.BrandProfile.PrimaryColor,
        accentColor: currentWorkspace.BrandProfile.AccentColor,
        selectedProgrammeId: selectedProgrammeId,
        baseUri: BuildBaseUri(httpRequest),
        cancellationToken: cancellationToken);

    return update is null ? Results.NotFound() : Results.Ok(update);
});

api.MapPost("/merchants/{merchantId:guid}/programmes", (
    Guid merchantId,
    CreateProgrammeApiRequest request,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var workspace = platformState.CreateProgramme(
        merchantId,
        request.TemplateKey,
        BuildBaseUri(httpRequest));

    return workspace is null ? Results.NotFound() : Results.Ok(workspace);
});

api.MapPut("/merchants/{merchantId:guid}/programmes/{programmeId:guid}", (
    Guid merchantId,
    Guid programmeId,
    UpdateProgrammeApiRequest request,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var workspace = platformState.UpdateProgramme(
        merchantId,
        programmeId,
        request.RewardItemLabel,
        request.RewardThreshold,
        request.RewardCopy,
        request.StartsOn,
        request.EndsOn,
        BuildBaseUri(httpRequest));

    return workspace is null ? Results.NotFound() : Results.Ok(workspace);
});

api.MapPost("/merchants/{merchantId:guid}/programmes/{programmeId:guid}/award-visit", (
    Guid merchantId,
    Guid programmeId,
    AwardVisitApiRequest request,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var workspace = platformState.AwardVisit(
        merchantId,
        programmeId,
        request.ScannedCode,
        BuildBaseUri(httpRequest));

    return workspace is null ? Results.NotFound() : Results.Ok(workspace);
});

api.MapPost("/merchants/{merchantId:guid}/programmes/{programmeId:guid}/cards/{cardId:guid}/redeem", (
    Guid merchantId,
    Guid programmeId,
    Guid cardId,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var workspace = platformState.RedeemReward(
        merchantId,
        programmeId,
        cardId,
        BuildBaseUri(httpRequest));

    return workspace is null ? Results.NotFound() : Results.Ok(workspace);
});

api.MapPost("/merchants/{merchantId:guid}/programmes/{programmeId:guid}/cards/{cardId:guid}/lifecycle", (
    Guid merchantId,
    Guid programmeId,
    Guid cardId,
    CardLifecycleApiRequest request,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var workspace = platformState.ApplyCardLifecycleAction(
        merchantId,
        programmeId,
        cardId,
        request.Action,
        BuildBaseUri(httpRequest));

    return workspace is null ? Results.NotFound() : Results.Ok(workspace);
});

api.MapPost("/merchants/{merchantId:guid}/programmes/{programmeId:guid}/cards/lifecycle", (
    Guid merchantId,
    Guid programmeId,
    BulkCardLifecycleApiRequest request,
    HttpContext context,
    HttpRequest httpRequest,
    DemoPlatformState platformState) =>
{
    var accessResult = EnsureMerchantApiAccess(context, merchantId);
    if (accessResult is not null)
    {
        return accessResult;
    }

    var workspace = platformState.ApplyBulkCardLifecycleAction(
        merchantId,
        programmeId,
        request.CardIds,
        request.Action,
        BuildBaseUri(httpRequest));

    return workspace is null ? Results.NotFound() : Results.Ok(workspace);
});

api.MapGet("/join/{joinCode}", (string joinCode, DemoPlatformState platformState) =>
{
    var experience = platformState.GetJoinExperience(joinCode);
    return experience is null ? Results.NotFound() : Results.Ok(experience);
});

api.MapPost("/join/{joinCode}", (string joinCode, DemoPlatformState platformState) =>
{
    var card = platformState.JoinCustomer(joinCode);
    return card is null ? Results.NotFound() : Results.Ok(card);
});

api.MapGet("/wallet/cards/{cardId:guid}", (Guid cardId, DemoPlatformState platformState) =>
{
    var card = platformState.GetWalletCard(cardId);
    return card is null ? Results.NotFound() : Results.Ok(card);
});

api.MapGet("/session/current", (HttpContext context) =>
{
    if (!(context.User.Identity?.IsAuthenticated ?? false))
    {
        return Results.Ok(new SessionSummaryResponse(false, null));
    }

    var merchantId = MerchantSessionClaims.TryGetMerchantId(context.User);
    if (!merchantId.HasValue)
    {
        return Results.Ok(new SessionSummaryResponse(false, null));
    }

    var summary = new SessionMerchantSummary(
        merchantId.Value,
        context.User.FindFirstValue(MerchantSessionClaims.MerchantEmail) ?? string.Empty,
        context.User.FindFirstValue(MerchantSessionClaims.MerchantName) ?? string.Empty);

    return Results.Ok(new SessionSummaryResponse(true, summary));
});

api.MapPost("/session/login", async (
    ApiLoginRequest request,
    HttpContext context,
    DemoPlatformState platformState) =>
{
    var result = platformState.AuthenticateMerchant(request.Email, request.Password);
    if (result.Status != MerchantAuthenticationStatus.Authenticated || result.Merchant is null)
    {
        return Results.Unauthorized();
    }

    await SignInMerchantAsync(context, result.Merchant);

    return Results.Ok(new SessionSummaryResponse(
        true,
        new SessionMerchantSummary(
            result.Merchant.MerchantId,
            result.Merchant.ContactEmail,
            result.Merchant.DisplayName)));
});

api.MapPost("/session/complete-signup", async (
    ApiCompleteSignupRequest request,
    HttpContext context,
    DemoPlatformState platformState) =>
{
    if (!string.Equals(request.Plan, "starter", StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest(new ApiErrorResponse("unsupported-plan", "Only starter is currently supported."));
    }

    if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
    {
        return Results.BadRequest(new ApiErrorResponse("password-mismatch", "Password and confirmation do not match."));
    }

    var result = platformState.ConfigureMerchantAccess(request.MerchantId, request.Password);
    if (result.Status != MerchantCredentialConfigurationStatus.Updated || result.Merchant is null)
    {
        var error = result.Status switch
        {
            MerchantCredentialConfigurationStatus.NotFound => "merchant-not-found",
            MerchantCredentialConfigurationStatus.AlreadyConfigured => "credentials-already-configured",
            _ => "invalid-password"
        };

        return Results.BadRequest(new ApiErrorResponse(error, "Unable to configure merchant credentials."));
    }

    await SignInMerchantAsync(context, result.Merchant);
    return Results.Ok(new SessionSummaryResponse(
        true,
        new SessionMerchantSummary(
            result.Merchant.MerchantId,
            result.Merchant.ContactEmail,
            result.Merchant.DisplayName)));
});

api.MapPost("/session/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

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

static string BuildBaseUri(HttpRequest request) =>
    $"{request.Scheme}://{request.Host}/";

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

static string BuildLoginRedirectUrl(HttpRequest request)
{
    var returnUrl = request.Path + request.QueryString;
    return BuildLoginUrl(returnUrl);
}

static string BuildLoginUrl(string? returnUrl, string? error = null, string? email = null)
{
    var query = new List<string>();
    if (!string.IsNullOrWhiteSpace(returnUrl))
    {
        query.Add($"returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    if (!string.IsNullOrWhiteSpace(error))
    {
        query.Add($"error={Uri.EscapeDataString(error)}");
    }

    if (!string.IsNullOrWhiteSpace(email))
    {
        query.Add($"email={Uri.EscapeDataString(email)}");
    }

    return query.Count == 0
        ? "/account/login"
        : $"/account/login?{string.Join("&", query)}";
}

static string? NormalizeLocalReturnUrl(string? returnUrl) =>
    !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith("/", StringComparison.Ordinal)
        ? returnUrl
        : null;

static bool TryGetMerchantRouteId(PathString path, out Guid merchantId)
{
    merchantId = default;

    var segments = path.Value?
        .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? [];

    return segments.Length >= 3
        && string.Equals(segments[0], "portal", StringComparison.OrdinalIgnoreCase)
        && string.Equals(segments[1], "merchant", StringComparison.OrdinalIgnoreCase)
        && Guid.TryParse(segments[2], out merchantId);
}

static IResult? EnsureMerchantApiAccess(HttpContext context, Guid merchantId)
{
    if (!(context.User.Identity?.IsAuthenticated ?? false))
    {
        return Results.Unauthorized();
    }

    var signedInMerchantId = MerchantSessionClaims.TryGetMerchantId(context.User);
    if (!signedInMerchantId.HasValue || signedInMerchantId.Value != merchantId)
    {
        return Results.Forbid();
    }

    return null;
}

static string BuildFallbackLogo(string displayName, string primaryColor, string accentColor)
{
    var initials = GetInitials(displayName);
    var svg =
        $"""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 160 160" role="img" aria-label="{displayName}">
          <defs>
            <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
              <stop offset="0%" stop-color="{NormalizeColor(primaryColor)}" />
              <stop offset="100%" stop-color="{NormalizeColor(accentColor)}" />
            </linearGradient>
          </defs>
          <rect width="160" height="160" rx="34" fill="url(#g)" />
          <text x="50%" y="54%" text-anchor="middle" dominant-baseline="middle" font-size="58" font-family="Trebuchet MS, sans-serif" fill="#fff8e3">{initials}</text>
        </svg>
        """;

    return $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(svg))}";
}

static string GetInitials(string name)
{
    var initials = name
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Take(2)
        .Select(part => char.ToUpperInvariant(part[0]))
        .ToArray();

    return initials.Length == 0 ? "F" : new string(initials);
}

static string NormalizeColor(string value) =>
    value.StartsWith('#') ? value : $"#{value}";

static async Task SignInMerchantAsync(HttpContext context, MerchantAccountSnapshot merchant)
{
    var claims = new List<Claim>
    {
        new(MerchantSessionClaims.MerchantId, merchant.MerchantId.ToString("D")),
        new(MerchantSessionClaims.MerchantEmail, merchant.ContactEmail),
        new(MerchantSessionClaims.MerchantName, merchant.DisplayName)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true,
            IssuedUtc = DateTimeOffset.UtcNow
        });
}

public sealed record WalletPassRegistrationRequest(string PushToken);

public sealed record WalletPassSerialUpdateResponse(string? LastUpdated, IReadOnlyList<string> SerialNumbers);

public sealed record WalletPassLogRequest(IReadOnlyList<string>? Logs);

public sealed record ApiErrorResponse(
    string Code,
    string Message);

public sealed record CreateMerchantApiRequest(
    string DisplayName,
    string ContactEmail,
    string? OwnerName = null,
    string? ShopTypeKey = null,
    string? TownOrCity = null,
    string? Postcode = null,
    string? PrimaryColor = null,
    string? AccentColor = null);

public sealed record UpdateBrandApiRequest(
    string DisplayName,
    string TownOrCity,
    string Postcode,
    string ContactEmail,
    string ShopTypeKey,
    string PrimaryColor,
    string AccentColor,
    Guid? SelectedProgrammeId = null);

public sealed record CreateProgrammeApiRequest(
    string TemplateKey);

public sealed record UpdateProgrammeApiRequest(
    string RewardItemLabel,
    int RewardThreshold,
    string RewardCopy,
    DateOnly StartsOn,
    DateOnly EndsOn);

public sealed record AwardVisitApiRequest(
    string ScannedCode);

public sealed record CardLifecycleApiRequest(
    string Action);

public sealed record BulkCardLifecycleApiRequest(
    string Action,
    IReadOnlyList<Guid> CardIds);

public sealed record ApiLoginRequest(
    string Email,
    string Password);

public sealed record ApiCompleteSignupRequest(
    Guid MerchantId,
    string Plan,
    string Password,
    string ConfirmPassword);

public sealed record SessionMerchantSummary(
    Guid MerchantId,
    string ContactEmail,
    string DisplayName);

public sealed record SessionSummaryResponse(
    bool IsAuthenticated,
    SessionMerchantSummary? Merchant);

public partial class Program;
