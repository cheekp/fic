"use client";

import Image from "next/image";
import { useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { RotateCcw, Sparkles } from "lucide-react";
import { Dialog, DialogContent, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
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
  interactive?: boolean;
  expandable?: boolean;
  flippable?: boolean;
  backTitle?: string;
  backDetails?: string[];
};

function CardSurface({
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
  variant,
}: Omit<LoyaltyCardPreviewProps, "className" | "testId" | "interactive" | "backTitle" | "backDetails">) {
  const isCompact = variant === "compact";

  return (
    <>
      <div
        className="absolute inset-0 opacity-95"
        style={{
          background: `radial-gradient(circle at 16% 18%, color-mix(in srgb, ${accentColor} 34%, white 66%) 0%, transparent 24%), radial-gradient(circle at 82% 84%, color-mix(in srgb, ${primaryColor} 78%, black 22%) 0%, transparent 34%), linear-gradient(145deg, color-mix(in srgb, ${primaryColor} 80%, #08111d 20%) 0%, color-mix(in srgb, ${primaryColor} 62%, black 38%) 46%, color-mix(in srgb, ${accentColor} 42%, #121a1f 58%) 100%)`,
        }}
      />
      <div
        className="absolute -left-[10%] top-[10%] h-[70%] w-[74%] rounded-full opacity-70"
        style={{
          background: `linear-gradient(135deg, color-mix(in srgb, ${accentColor} 70%, white 30%) 0%, transparent 70%)`,
          filter: "blur(18px)",
        }}
      />
      <div
        className="absolute -right-[4%] top-0 h-full w-[42%] opacity-80"
        style={{
          background: "linear-gradient(180deg, rgba(255,255,255,0.18), rgba(255,255,255,0.03) 32%, rgba(255,255,255,0.12) 100%)",
          transform: "skewX(-22deg)",
        }}
      />
      <div className="absolute inset-x-[7%] top-[7%] h-[34%] rounded-full bg-[linear-gradient(180deg,rgba(255,255,255,0.34),rgba(255,255,255,0.02))] opacity-70 blur-[2px]" />
      <div className="absolute inset-[1px] rounded-[inherit] border border-white/10" />
      <div className="absolute inset-0 rounded-[inherit] shadow-[inset_0_1px_0_rgba(255,255,255,0.22),inset_0_-20px_48px_rgba(3,8,16,0.28)]" />
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top,rgba(255,255,255,0.24),transparent_26%),linear-gradient(180deg,transparent,rgba(3,8,16,0.34))]" />

      <div className={cn("relative z-10 flex h-full flex-col justify-between", isCompact ? "p-4" : "p-6 sm:p-7")}>
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <p className={cn("uppercase tracking-[0.22em] text-white/72", isCompact ? "text-[9px]" : "text-[11px]")}>
              {merchantName}
            </p>
            <h3 className={cn("mt-3 max-w-[13ch] font-display leading-[0.94] text-white", isCompact ? "text-[1.18rem]" : "text-[2.25rem]")}>
              {title}
            </h3>
          </div>
          {logoUrl ? (
            <span className={cn(
              "flex shrink-0 items-center justify-center overflow-hidden rounded-[1.15rem] border border-white/18 bg-white/10 backdrop-blur",
              isCompact ? "h-11 w-11" : "h-14 w-14",
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

        <div className="grid gap-3">
          <p className={cn("max-w-[28ch] text-white/82", isCompact ? "text-[10px] leading-[1.45]" : "text-sm leading-6")}>
            {subtitle}
          </p>
          <div className="flex items-end justify-between gap-3">
            <div>
              <p className={cn("uppercase tracking-[0.18em] text-white/56", isCompact ? "text-[8px]" : "text-[10px]")}>
                Live progress
              </p>
              <p className={cn("mt-1 font-semibold text-white", isCompact ? "text-[0.95rem]" : "text-[1.4rem]")}>
                {progressLabel}
              </p>
            </div>
            {metaLabel ? (
              <span className={cn(
                "rounded-full border border-white/16 bg-white/9 px-3 py-1.5 font-medium text-white/86 backdrop-blur",
                isCompact ? "text-[9px]" : "text-xs",
              )}>
                {metaLabel}
              </span>
            ) : null}
          </div>
        </div>
      </div>
    </>
  );
}

function CardBack({
  merchantName,
  backTitle,
  backDetails,
  accentColor,
}: {
  merchantName: string;
  backTitle: string;
  backDetails: string[];
  accentColor: string;
}) {
  return (
    <div className="absolute inset-0 overflow-hidden rounded-[inherit] bg-[linear-gradient(180deg,rgba(9,17,29,0.98),rgba(15,27,42,0.94))]">
      <div
        className="absolute inset-0 opacity-60"
        style={{
          background: `radial-gradient(circle at top right, color-mix(in srgb, ${accentColor} 28%, white 72%), transparent 30%)`,
        }}
      />
      <div className="relative flex h-full flex-col justify-between p-5 sm:p-6">
        <div className="flex items-center justify-between gap-3">
          <div>
            <p className="text-[11px] uppercase tracking-[0.2em] text-white/58">{merchantName}</p>
            <h3 className="mt-2 font-display text-[1.6rem] leading-none text-white">{backTitle}</h3>
          </div>
          <Sparkles className="h-5 w-5 text-white/72" />
        </div>
        <div className="space-y-2.5">
          {backDetails.map((detail) => (
            <div key={detail} className="rounded-xl border border-white/10 bg-white/6 px-3 py-2 text-sm text-white/86 backdrop-blur">
              {detail}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

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
  interactive = false,
  expandable,
  flippable,
  backTitle,
  backDetails = [],
}: LoyaltyCardPreviewProps) {
  const [isFlipped, setIsFlipped] = useState(false);
  const isCompact = variant === "compact";
  const isExpandable = expandable ?? interactive;
  const isFlippable = flippable ?? interactive;
  const sharedClassName = cn(
    "group relative isolate w-full overflow-hidden border text-white shadow-[0_30px_80px_-42px_rgba(7,15,28,0.78)]",
    isCompact ? "aspect-[1.58/1] rounded-[1.45rem]" : "aspect-[1.58/1] rounded-[2rem]",
    className,
  );

  const card = (
    <motion.div
      data-testid={testId}
      className={sharedClassName}
      style={{
        borderColor: "rgba(255,255,255,0.14)",
        background: `linear-gradient(138deg, ${primaryColor} 0%, color-mix(in srgb, ${primaryColor} 72%, #08111d 28%) 48%, ${accentColor} 100%)`,
        transformStyle: "preserve-3d",
      }}
      whileHover={isExpandable || isFlippable ? { y: -4, scale: 1.01 } : undefined}
      transition={{ duration: 0.22, ease: [0.22, 0.9, 0.2, 1] }}
    >
      <AnimatePresence initial={false} mode="wait">
        {!isFlipped ? (
          <motion.div
            key="front"
            className="absolute inset-0"
            initial={{ rotateY: -90, opacity: 0 }}
            animate={{ rotateY: 0, opacity: 1 }}
            exit={{ rotateY: 90, opacity: 0 }}
            transition={{ duration: 0.32 }}
          >
            <CardSurface
              merchantName={merchantName}
              title={title}
              subtitle={subtitle}
              progressLabel={progressLabel}
              metaLabel={metaLabel}
              logoUrl={logoUrl}
              logoWidth={logoWidth}
              logoHeight={logoHeight}
              primaryColor={primaryColor}
              accentColor={accentColor}
              variant={variant}
            />
          </motion.div>
        ) : (
          <motion.div
            key="back"
            className="absolute inset-0"
            initial={{ rotateY: -90, opacity: 0 }}
            animate={{ rotateY: 0, opacity: 1 }}
            exit={{ rotateY: 90, opacity: 0 }}
            transition={{ duration: 0.32 }}
          >
            <CardBack
              merchantName={merchantName}
              backTitle={backTitle ?? title}
              backDetails={backDetails.length > 0 ? backDetails : [subtitle, progressLabel]}
              accentColor={accentColor}
            />
          </motion.div>
        )}
      </AnimatePresence>

      {isFlippable ? (
        <button
          type="button"
          onClick={(event) => {
            event.preventDefault();
            event.stopPropagation();
            setIsFlipped((current) => !current);
          }}
          className="absolute bottom-3 right-3 z-20 inline-flex items-center gap-1 rounded-full border border-white/18 bg-white/10 px-2.5 py-1 text-[10px] font-medium uppercase tracking-[0.16em] text-white/88 backdrop-blur"
        >
          <RotateCcw className="h-3 w-3" />
          Flip
        </button>
      ) : null}
    </motion.div>
  );

  if (!isExpandable) {
    return card;
  }

  return (
    <Dialog>
      <DialogTrigger asChild>
        <div
          role="button"
          tabIndex={0}
          className="block w-full cursor-pointer text-left"
          onKeyDown={(event) => {
            if (event.key === "Enter" || event.key === " ") {
              event.preventDefault();
              event.currentTarget.click();
            }
          }}
        >
          {card}
        </div>
      </DialogTrigger>
      <DialogContent className="border-[rgba(15,27,42,0.12)] bg-[rgba(255,251,245,0.98)] p-4 shadow-[0_32px_90px_-44px_rgba(15,27,42,0.46)] sm:max-w-4xl">
        <DialogTitle className="sr-only">{title}</DialogTitle>
        <div className="mx-auto w-full max-w-3xl py-3">
          <LoyaltyCardPreview
            merchantName={merchantName}
            title={title}
            subtitle={subtitle}
            progressLabel={progressLabel}
            metaLabel={metaLabel}
            logoUrl={logoUrl}
            logoWidth={logoWidth}
            logoHeight={logoHeight}
            primaryColor={primaryColor}
            accentColor={accentColor}
            variant="hero"
            flippable={isFlippable}
            expandable={false}
            backTitle={backTitle}
            backDetails={backDetails}
          />
        </div>
      </DialogContent>
    </Dialog>
  );
}
