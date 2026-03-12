using Fic.Platform.Web.Components;
using Fic.Platform.Web.Services;
using Fic.WalletPasses;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseStaticWebAssets();
}

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DemoPlatformState>();
builder.Services.AddSingleton<JoinQrCodeService>();
builder.Services.AddSingleton<IAppleWalletPassService>(_ =>
{
    var options = new AppleWalletPassOptions();
    builder.Configuration.GetSection("Wallet:AppleWallet").Bind(options);
    options.SigningConfigured = builder.Configuration.GetValue("Wallet:AppleWalletSigningConfigured", false);
    options.DefaultAssetDirectory = Path.Combine(
        builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot"),
        "wallet-assets");
    return new AppleWalletPassService(options);
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
