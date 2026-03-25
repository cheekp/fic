import type { CSSProperties } from "react";
import type { PortalThemeContract } from "@/types/portal-contracts";
import { ficPortalTheme, northStarPortalTheme } from "@/types/portal-contracts";

type BrandInput = {
  primaryColor?: string | null;
  accentColor?: string | null;
};

function clamp(value: number, min: number, max: number) {
  return Math.min(max, Math.max(min, value));
}

function normalizeHex(input: string | null | undefined, fallback: string) {
  if (!input) {
    return fallback;
  }

  const trimmed = input.trim();
  const shortHexMatch = /^#([\da-f]{3})$/i.exec(trimmed);
  if (shortHexMatch) {
    const [r, g, b] = shortHexMatch[1].split("");
    return `#${r}${r}${g}${g}${b}${b}`.toLowerCase();
  }

  const longHexMatch = /^#([\da-f]{6})$/i.exec(trimmed);
  if (longHexMatch) {
    return `#${longHexMatch[1].toLowerCase()}`;
  }

  return fallback;
}

function hexToRgb(hex: string) {
  const normalized = normalizeHex(hex, ficPortalTheme.primary);
  const value = normalized.slice(1);

  return {
    r: Number.parseInt(value.slice(0, 2), 16),
    g: Number.parseInt(value.slice(2, 4), 16),
    b: Number.parseInt(value.slice(4, 6), 16),
  };
}

function rgbToHslTriplet(red: number, green: number, blue: number) {
  const r = red / 255;
  const g = green / 255;
  const b = blue / 255;
  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);
  const lightness = (max + min) / 2;
  const delta = max - min;

  if (delta === 0) {
    return `0 0% ${Math.round(lightness * 100)}%`;
  }

  const saturation = lightness > 0.5
    ? delta / (2 - max - min)
    : delta / (max + min);

  let hue = 0;
  switch (max) {
    case r:
      hue = (g - b) / delta + (g < b ? 6 : 0);
      break;
    case g:
      hue = (b - r) / delta + 2;
      break;
    default:
      hue = (r - g) / delta + 4;
      break;
  }

  hue /= 6;

  return `${Math.round(hue * 360)} ${Math.round(saturation * 100)}% ${Math.round(lightness * 100)}%`;
}

function hexToHslTriplet(hex: string) {
  const { r, g, b } = hexToRgb(hex);
  return rgbToHslTriplet(r, g, b);
}

function toHex(value: number) {
  return clamp(Math.round(value), 0, 255).toString(16).padStart(2, "0");
}

function mixHex(left: string, right: string, weight: number) {
  const ratio = clamp(weight, 0, 1);
  const a = hexToRgb(left);
  const b = hexToRgb(right);

  return `#${toHex(a.r + (b.r - a.r) * ratio)}${toHex(a.g + (b.g - a.g) * ratio)}${toHex(a.b + (b.b - a.b) * ratio)}`;
}

function relativeLuminance(hex: string) {
  const { r, g, b } = hexToRgb(hex);
  const channels = [r, g, b].map((channel) => {
    const value = channel / 255;
    return value <= 0.03928 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4;
  });

  return 0.2126 * channels[0] + 0.7152 * channels[1] + 0.0722 * channels[2];
}

function pickReadableInk(backgroundHex: string, darkInk: string, lightInk: string) {
  return relativeLuminance(backgroundHex) > 0.45 ? darkInk : lightInk;
}

function rgbaToHex(input: string, fallback: string) {
  const match = /rgba?\(([^)]+)\)/.exec(input);
  if (!match) {
    return fallback;
  }

  const [red, green, blue] = match[1]
    .split(",")
    .slice(0, 3)
    .map((value) => Number.parseInt(value.trim(), 10));

  if ([red, green, blue].some((value) => Number.isNaN(value))) {
    return fallback;
  }

  return `#${toHex(red)}${toHex(green)}${toHex(blue)}`;
}

export function resolvePortalBrandTheme(brand?: BrandInput | null): PortalThemeContract {
  if (!brand) {
    return northStarPortalTheme;
  }

  const primary = normalizeHex(brand.primaryColor, ficPortalTheme.primary);
  const accent = normalizeHex(brand.accentColor, ficPortalTheme.accent);
  const surfaceBase = mixHex("#fffaf2", primary, 0.08);
  const surfaceHighlight = mixHex("#fffdf9", accent, 0.06);
  const ink = pickReadableInk(surfaceBase, "#14211d", "#f8f4ea");
  const useDarkChrome = false;
  const surface = `color-mix(in srgb, ${surfaceBase} 88%, white 12%)`;
  const surfaceStrong = `color-mix(in srgb, ${surfaceHighlight} 94%, white 6%)`;
  const line = "rgba(20, 33, 29, 0.1)";
  const mutedInk = "rgba(20, 33, 29, 0.72)";
  const logoPlate = `color-mix(in srgb, ${accent} 14%, white 86%)`;
  const logoPlateBorder = `color-mix(in srgb, ${primary} 10%, white 90%)`;

  return {
    primary,
    accent,
    surface,
    surfaceStrong,
    ink,
    mutedInk,
    line,
    canvasStart: mixHex("#fff9ef", primary, 0.04),
    canvasEnd: mixHex("#f4ede1", accent, 0.1),
    primaryButton: primary,
    primaryButtonInk: pickReadableInk(primary, "#f8f4ea", "#14211d"),
    accentSoft: `color-mix(in srgb, ${accent} 14%, transparent 86%)`,
    accentInk: pickReadableInk(accent, "#14211d", "#fffaf2"),
    logoPlate,
    logoPlateBorder,
    stampFilled: accent,
    stampEmpty: `color-mix(in srgb, ${primary} 8%, transparent 92%)`,
    stampInk: pickReadableInk(accent, "#14211d", "#fffaf2"),
    glow: `color-mix(in srgb, ${accent} 12%, transparent 88%)`,
    variant: "bloom",
    useDarkChrome,
    radius: "rounded",
    shadow: "soft",
  };
}

export function buildBrandCssVars(theme: PortalThemeContract): CSSProperties {
  const canvasStart = normalizeHex(theme.canvasStart, mixHex(theme.primary, "#fff9ef", 0.9));
  const canvasEnd = normalizeHex(theme.canvasEnd, mixHex(theme.accent, "#f4ede1", 0.78));
  const card = normalizeHex(theme.useDarkChrome ? mixHex(theme.primary, "#ffffff", 0.18) : mixHex(theme.primary, "#ffffff", 0.95), mixHex(theme.primary, "#ffffff", 0.95));
  const border = normalizeHex(theme.useDarkChrome ? mixHex(theme.primary, "#ffffff", 0.2) : mixHex(theme.primary, "#d9ccba", 0.8), mixHex(theme.primary, "#d9ccba", 0.8));
  const shadow = theme.shadow === "soft" ? "rgba(16, 24, 21, 0.16)" : "rgba(16, 24, 21, 0.24)";
  const mutedInkHex = rgbaToHex(theme.mutedInk, mixHex(theme.ink, "#ffffff", 0.35));

  return {
    ["--font-display" as string]: '"Iowan Old Style", "Palatino Linotype", "Book Antiqua", serif',
    ["--font-body" as string]: '"Avenir Next", "Segoe UI", sans-serif',
    ["--brand-primary-hex" as string]: theme.primary,
    ["--brand-accent-hex" as string]: theme.accent,
    ["--brand-ink-hex" as string]: theme.ink,
    ["--brand-canvas-start-hex" as string]: canvasStart,
    ["--brand-canvas-end-hex" as string]: canvasEnd,
    ["--background" as string]: hexToHslTriplet(canvasStart),
    ["--foreground" as string]: hexToHslTriplet(theme.ink),
    ["--card" as string]: hexToHslTriplet(card),
    ["--card-foreground" as string]: hexToHslTriplet(theme.ink),
    ["--primary" as string]: hexToHslTriplet(theme.primary),
    ["--primary-foreground" as string]: pickReadableInk(theme.primaryButton, "#f8f4ea", "#14211d") === "#f8f4ea" ? "43 40% 95%" : "157 27% 14%",
    ["--secondary" as string]: hexToHslTriplet(theme.accent),
    ["--secondary-foreground" as string]: theme.accentInk === "#fffaf2" ? "43 40% 95%" : "157 27% 14%",
    ["--muted" as string]: hexToHslTriplet(card),
    ["--muted-foreground" as string]: hexToHslTriplet(mutedInkHex),
    ["--accent" as string]: hexToHslTriplet(theme.accent),
    ["--accent-foreground" as string]: theme.accentInk === "#fffaf2" ? "43 40% 95%" : "157 27% 14%",
    ["--destructive" as string]: "0 74% 52%",
    ["--destructive-foreground" as string]: "0 0% 98%",
    ["--border" as string]: hexToHslTriplet(border),
    ["--input" as string]: hexToHslTriplet(border),
    ["--ring" as string]: hexToHslTriplet(theme.primary),
    ["--radius" as string]: "1rem",
    ["--shadow" as string]: shadow,
    ["--brand-muted-ink" as string]: theme.mutedInk,
    ["--brand-line" as string]: theme.line,
    ["--brand-button" as string]: theme.primaryButton,
    ["--brand-button-ink" as string]: theme.primaryButtonInk,
    ["--brand-accent-soft" as string]: theme.accentSoft,
    ["--brand-accent-ink" as string]: theme.accentInk,
    ["--brand-stamp-filled" as string]: theme.stampFilled,
    ["--brand-stamp-empty" as string]: theme.stampEmpty,
    ["--brand-stamp-ink" as string]: theme.stampInk,
    ["--brand-glow" as string]: theme.glow,
    ["--brand-surface-strong" as string]: theme.surfaceStrong,
    ["--brand-title-glow" as string]: theme.glow,
    ["--brand-hero-start" as string]: theme.canvasStart,
    ["--brand-hero-end" as string]: theme.canvasEnd,
    ["--brand-hero-panel" as string]: theme.surfaceStrong,
    ["--brand-logo-plate" as string]: theme.logoPlate,
    ["--brand-logo-border" as string]: theme.logoPlateBorder,
  };
}
