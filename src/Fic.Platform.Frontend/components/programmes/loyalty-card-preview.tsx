"use client";

import Image from "next/image";
import { cn } from "@/lib/utils";

type LoyaltyCardPreviewProps = {
  merchantName: string;
  title: string;
  subtitle: string;
  progressLabel: string;
  metaLabel?: string;
  logoUrl?: string | null;
  logoWidth?: number;
  logoHeight?: number;
  primaryColor: string;
  accentColor: string;
  variant?: "hero" | "compact";
  className?: string;
  testId?: string;
};

export function LoyaltyCardPreview({
  merchantName,
  title,
  subtitle,
  progressLabel,
  metaLabel,
  logoUrl,
  logoWidth = 72,
  logoHeight = 72,
  primaryColor,
  accentColor,
  variant = "hero",
  className,
  testId,
}: LoyaltyCardPreviewProps) {
  const isCompact = variant === "compact";

  return (
    <div
      data-testid={testId}
      className={cn(
        "group relative isolate overflow-hidden rounded-[1.7rem] border text-white shadow-[0_28px_72px_-44px_rgba(7,15,28,0.82)]",
        isCompact ? "h-28 w-full rounded-[1.2rem]" : "min-h-[18rem] w-full rounded-[2rem]",
        className,
      )}
      style={{
        borderColor: "rgba(255,255,255,0.14)",
        background: `linear-gradient(138deg, ${primaryColor} 0%, color-mix(in srgb, ${primaryColor} 72%, #08111d 28%) 48%, ${accentColor} 100%)`,
      }}
    >
      <div
        className="absolute inset-0 opacity-90"
        style={{
          background: `radial-gradient(circle at 12% 18%, color-mix(in srgb, ${accentColor} 38%, white 62%) 0%, transparent 28%), radial-gradient(circle at 82% 88%, color-mix(in srgb, ${primaryColor} 74%, black 26%) 0%, transparent 38%)`,
        }}
      />
      <div
        className="absolute -left-[12%] top-[14%] h-[78%] w-[78%] rounded-full opacity-75"
        style={{
          background: `linear-gradient(135deg, color-mix(in srgb, ${accentColor} 72%, white 28%) 0%, transparent 74%)`,
          filter: "blur(12px)",
        }}
      />
      <div
        className="absolute -right-[8%] top-0 h-full w-[46%] opacity-75"
        style={{
          background: "linear-gradient(180deg, rgba(255,255,255,0.18), rgba(255,255,255,0.02) 42%, rgba(255,255,255,0.08) 100%)",
          transform: "skewX(-18deg)",
        }}
      />
      <div className="absolute inset-[1px] rounded-[inherit] border border-white/8" />
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top,rgba(255,255,255,0.22),transparent_28%),linear-gradient(180deg,transparent,rgba(3,8,16,0.34))]" />

      <div className={cn("relative z-10 flex h-full flex-col justify-between", isCompact ? "p-3.5" : "p-5 sm:p-6")}>
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <p className={cn("uppercase tracking-[0.2em] text-white/74", isCompact ? "text-[9px]" : "text-[11px]")}>
              {merchantName}
            </p>
            <h3 className={cn("mt-2 max-w-[16ch] font-display leading-none text-white", isCompact ? "text-base" : "text-[2rem]")}>
              {title}
            </h3>
          </div>
          {logoUrl ? (
            <span className={cn(
              "flex shrink-0 items-center justify-center overflow-hidden rounded-2xl border border-white/20 bg-white/12 backdrop-blur",
              isCompact ? "h-10 w-10" : "h-14 w-14",
            )}>
              <Image
                src={logoUrl}
                alt={`${merchantName} logo`}
                width={Math.max(logoWidth, isCompact ? 40 : 56)}
                height={Math.max(logoHeight, isCompact ? 40 : 56)}
                className="h-auto max-h-[80%] w-auto max-w-[80%]"
                unoptimized
              />
            </span>
          ) : null}
        </div>

        <div className="grid gap-2">
          <p className={cn("max-w-[30ch] text-white/84", isCompact ? "text-[10px] leading-4" : "text-sm leading-6")}>
            {subtitle}
          </p>
          <div className="flex items-end justify-between gap-3">
            <div>
              <p className={cn("uppercase tracking-[0.18em] text-white/58", isCompact ? "text-[8px]" : "text-[10px]")}>
                Live progress
              </p>
              <p className={cn("mt-1 font-semibold text-white", isCompact ? "text-sm" : "text-xl")}>
                {progressLabel}
              </p>
            </div>
            {metaLabel ? (
              <span className={cn(
                "rounded-full border border-white/18 bg-white/10 px-2.5 py-1 font-medium text-white/86 backdrop-blur",
                isCompact ? "text-[9px]" : "text-xs",
              )}>
                {metaLabel}
              </span>
            ) : null}
          </div>
        </div>
      </div>
    </div>
  );
}
