namespace Fic.MerchantAccounts;

public sealed record MerchantLogoMetadata(
    int Width,
    int Height)
{
    public double AspectRatio => Height <= 0 ? 1d : (double)Width / Height;
}

public sealed record MerchantBrandTheme(
    string Variant,
    bool UseDarkChrome,
    string PrimaryColor,
    string AccentColor,
    string CanvasStartColor,
    string CanvasEndColor,
    string SurfaceColor,
    string SurfaceStrongColor,
    string InkColor,
    string MutedInkColor,
    string LineColor,
    string PrimaryButtonColor,
    string PrimaryButtonInkColor,
    string AccentSoftColor,
    string AccentInkColor,
    string LogoPlateColor,
    string LogoPlateBorderColor,
    string StampFilledColor,
    string StampEmptyColor,
    string StampInkColor,
    string GlowColor,
    string ShadowColor);

public static class MerchantBrandThemeCompiler
{
    public static MerchantBrandTheme Compile(
        string primaryColor,
        string accentColor,
        MerchantLogoMetadata logoMetadata)
    {
        var primary = ColorRgb.Parse(primaryColor);
        var accent = ColorRgb.Parse(accentColor);
        var useDarkChrome = primary.RelativeLuminance < 0.5;
        var accentSaturation = accent.Saturation;
        var luminanceDelta = Math.Abs(primary.RelativeLuminance - accent.RelativeLuminance);

        var variant = logoMetadata.AspectRatio >= 1.35
            ? "ribbon"
            : useDarkChrome && accentSaturation >= 0.42 && luminanceDelta >= 0.1
                ? "glow"
                : "bloom";

        var canvasStart = useDarkChrome
            ? primary.Mix(ColorRgb.Black, 0.52)
            : primary.Mix(ColorRgb.White, 0.84);
        var canvasEnd = useDarkChrome
            ? primary.Mix(accent, 0.24).Mix(ColorRgb.Black, 0.32)
            : primary.Mix(accent, 0.12).Mix(ColorRgb.White, 0.72);
        var surface = useDarkChrome
            ? primary.Mix(ColorRgb.White, 0.12).ToRgba(0.84)
            : primary.Mix(ColorRgb.White, 0.92).ToRgba(0.88);
        var surfaceStrong = useDarkChrome
            ? primary.Mix(accent, 0.16).Mix(ColorRgb.White, 0.18).ToRgba(0.96)
            : primary.Mix(accent, 0.08).Mix(ColorRgb.White, 0.96).ToRgba(0.96);
        var ink = useDarkChrome
            ? ColorRgb.FromHex("#fff7ec")
            : primary.Mix(ColorRgb.Black, 0.82);
        var mutedInk = useDarkChrome
            ? ink.ToRgba(0.78)
            : primary.Mix(ColorRgb.Black, 0.62).ToRgba(0.74);
        var line = useDarkChrome
            ? ColorRgb.White.ToRgba(0.14)
            : ink.ToRgba(0.1);
        var primaryButton = variant == "glow"
            ? accent.Mix(ColorRgb.Black, 0.08)
            : primary;
        var primaryButtonInk = primaryButton.ReadableInk();
        var accentSoft = accent.ToRgba(useDarkChrome ? 0.24 : 0.18);
        var accentInk = accent.ReadableInk();
        var logoPlate = useDarkChrome
            ? ColorRgb.White.ToRgba(0.07)
            : primary.Mix(ColorRgb.White, 0.95).ToRgba(0.96);
        var logoPlateBorder = useDarkChrome
            ? accent.ToRgba(0.22)
            : primary.ToRgba(0.12);
        var stampFill = accent;
        var stampEmpty = useDarkChrome
            ? ColorRgb.White.ToRgba(0.09)
            : primary.ToRgba(0.08);
        var stampInk = stampFill.ReadableInk();
        var glow = accent.ToRgba(useDarkChrome ? 0.34 : 0.16);
        var shadow = primary.Mix(ColorRgb.Black, 0.54).ToRgba(useDarkChrome ? 0.34 : 0.16);

        return new MerchantBrandTheme(
            variant,
            useDarkChrome,
            primary.ToHex(),
            accent.ToHex(),
            canvasStart.ToHex(),
            canvasEnd.ToHex(),
            surface,
            surfaceStrong,
            ink.ToHex(),
            mutedInk,
            line,
            primaryButton.ToHex(),
            primaryButtonInk.ToHex(),
            accentSoft,
            accentInk.ToHex(),
            logoPlate,
            logoPlateBorder,
            stampFill.ToHex(),
            stampEmpty,
            stampInk.ToHex(),
            glow,
            shadow);
    }

    private readonly record struct ColorRgb(byte R, byte G, byte B)
    {
        public static ColorRgb White => new(255, 255, 255);
        public static ColorRgb Black => new(0, 0, 0);

        public static ColorRgb Parse(string value)
        {
            var normalized = value.Trim();
            if (normalized.StartsWith('#'))
            {
                normalized = normalized[1..];
            }

            if (normalized.Length != 6)
            {
                throw new InvalidOperationException($"Brand colour '{value}' must be a 6 digit hex value.");
            }

            return new ColorRgb(
                Convert.ToByte(normalized[..2], 16),
                Convert.ToByte(normalized[2..4], 16),
                Convert.ToByte(normalized[4..6], 16));
        }

        public static ColorRgb FromHex(string value) => Parse(value);

        public double RelativeLuminance
        {
            get
            {
                static double Channel(byte channel)
                {
                    var normalized = channel / 255d;
                    return normalized <= 0.03928
                        ? normalized / 12.92
                        : Math.Pow((normalized + 0.055) / 1.055, 2.4);
                }

                return (0.2126 * Channel(R)) + (0.7152 * Channel(G)) + (0.0722 * Channel(B));
            }
        }

        public double Saturation
        {
            get
            {
                var max = Math.Max(R, Math.Max(G, B)) / 255d;
                var min = Math.Min(R, Math.Min(G, B)) / 255d;
                if (Math.Abs(max - min) < double.Epsilon)
                {
                    return 0d;
                }

                var lightness = (max + min) / 2d;
                return lightness > 0.5
                    ? (max - min) / (2d - max - min)
                    : (max - min) / (max + min);
            }
        }

        public ColorRgb Mix(ColorRgb other, double ratio)
        {
            var clamped = Math.Clamp(ratio, 0d, 1d);
            return new ColorRgb(
                (byte)Math.Round((R * (1d - clamped)) + (other.R * clamped)),
                (byte)Math.Round((G * (1d - clamped)) + (other.G * clamped)),
                (byte)Math.Round((B * (1d - clamped)) + (other.B * clamped)));
        }

        public ColorRgb ReadableInk() =>
            RelativeLuminance >= 0.52 ? Black.Mix(this, 0.12) : White;

        public string ToHex() => $"#{R:x2}{G:x2}{B:x2}";

        public string ToRgba(double alpha) =>
            $"rgba({R}, {G}, {B}, {alpha.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)})";
    }
}
