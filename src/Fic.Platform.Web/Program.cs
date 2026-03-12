using Fic.Platform.Web.Components;
using Fic.Platform.Web.Services;
using Fic.WalletPasses;
using Fic.MerchantAccounts;
using Microsoft.Extensions.FileProviders;
using Azure.Storage.Blobs;

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
builder.Services.AddSingleton<IAppleWalletPassService>(sp =>
{
    var options = new AppleWalletPassOptions();
    builder.Configuration.GetSection("Wallet:AppleWallet").Bind(options);
    options.SigningConfigured = builder.Configuration.GetValue("Wallet:AppleWalletSigningConfigured", false);
    options.DefaultAssetDirectory = Path.Combine(
        builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot"),
        "wallet-assets");
    return new AppleWalletPassService(options, sp.GetRequiredService<IMerchantBrandAssetStore>());
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
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

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
    DemoPlatformState platformState,
    IAppleWalletPassService walletPasses,
    CancellationToken cancellationToken) =>
{
    var card = platformState.GetWalletCard(cardId);
    if (card is null)
    {
        return Results.NotFound();
    }

    if (!walletPasses.GetCapability().SupportsAppleWalletPass)
    {
        return Results.Redirect($"/wallet/card/{cardId}");
    }

    var package = await walletPasses.CreatePackageAsync(card, cancellationToken);
    return Results.File(package.Bytes, package.ContentType, package.FileName);
});

app.MapStaticAssets();
app.MapDefaultEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
