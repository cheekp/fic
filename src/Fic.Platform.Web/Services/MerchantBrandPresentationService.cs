using System.Globalization;
using Fic.Contracts;
using Fic.MerchantAccounts;

namespace Fic.Platform.Web.Services;

public sealed class MerchantBrandPresentationService
{
    private static readonly MerchantLogoMetadata DefaultLogoMetadata = new(160, 160);
    private static readonly MerchantThemeView DefaultTheme = BuildTheme("#1f3731", "#f4c15d", DefaultLogoMetadata);

    public MerchantThemeView GetDefaultTheme() => DefaultTheme;

    public MerchantThemeView Build(BrandProfileSnapshot brand) =>
        BuildTheme(
            brand.PrimaryColor,
            brand.AccentColor,
            new MerchantLogoMetadata(brand.LogoWidth, brand.LogoHeight));

    public MerchantThemeView Build(WalletCardSnapshot card) =>
        BuildTheme(
            card.PrimaryColor,
            card.AccentColor,
            new MerchantLogoMetadata(card.LogoWidth, card.LogoHeight));

    public MerchantThemeView Build(MerchantSummarySnapshot merchant) =>
        BuildTheme(
            merchant.PrimaryColor,
            merchant.AccentColor,
            new MerchantLogoMetadata(merchant.LogoWidth, merchant.LogoHeight));

    public MerchantThemeView BuildPreview(
        string primaryColor,
        string accentColor,
        int logoWidth = 160,
        int logoHeight = 160) =>
        BuildTheme(primaryColor, accentColor, new MerchantLogoMetadata(logoWidth, logoHeight));

    private static MerchantThemeView BuildTheme(
        string primaryColor,
        string accentColor,
        MerchantLogoMetadata logoMetadata)
    {
        var theme = MerchantBrandThemeCompiler.Compile(primaryColor, accentColor, logoMetadata);
        var cssVariables = string.Join(
            "; ",
            [
                $"--brand-primary: {theme.PrimaryColor}",
                $"--brand-accent: {theme.AccentColor}",
                $"--brand-canvas-start: {theme.CanvasStartColor}",
                $"--brand-canvas-end: {theme.CanvasEndColor}",
                $"--brand-surface: {theme.SurfaceColor}",
                $"--brand-surface-strong: {theme.SurfaceStrongColor}",
                $"--brand-ink: {theme.InkColor}",
                $"--brand-muted: {theme.MutedInkColor}",
                $"--brand-line: {theme.LineColor}",
                $"--brand-button: {theme.PrimaryButtonColor}",
                $"--brand-button-ink: {theme.PrimaryButtonInkColor}",
                $"--brand-accent-soft: {theme.AccentSoftColor}",
                $"--brand-accent-ink: {theme.AccentInkColor}",
                $"--brand-logo-plate: {theme.LogoPlateColor}",
                $"--brand-logo-border: {theme.LogoPlateBorderColor}",
                $"--brand-stamp-fill: {theme.StampFilledColor}",
                $"--brand-stamp-empty: {theme.StampEmptyColor}",
                $"--brand-stamp-ink: {theme.StampInkColor}",
                $"--brand-glow: {theme.GlowColor}",
                $"--brand-shadow: {theme.ShadowColor}",
                $"--brand-logo-ratio: {logoMetadata.AspectRatio.ToString("0.###", CultureInfo.InvariantCulture)}"
            ]);

        return new MerchantThemeView(
            $"merchant-theme--{theme.Variant}",
            $"{cssVariables};",
            theme);
    }
}

public sealed record MerchantThemeView(
    string ThemeClass,
    string CssVariables,
    MerchantBrandTheme Theme);
