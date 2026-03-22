"use client";

import Link from "next/link";
import { Check, CheckCircle2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { PortalRoadmapContract } from "@/types/portal-contracts";

export type OnboardingJourneyKey = "account" | "plan" | "owner" | "billing" | "shop" | "programme";

type OnboardingJourneyProps = {
  roadmap?: PortalRoadmapContract | null;
  currentStep: OnboardingJourneyKey;
  accountComplete: boolean;
  planComplete: boolean;
  ownerComplete: boolean;
  billingComplete: boolean;
  shopComplete: boolean;
  programmeComplete: boolean;
  accountUrl?: string;
  planUrl?: string;
  ownerUrl?: string;
  billingUrl?: string;
  shopUrl?: string;
  programmeUrl?: string;
  hint?: string;
  variant?: "default" | "compact";
};

type JourneyStep = {
  order: number;
  key: OnboardingJourneyKey;
  label: string;
  compactLabel?: string;
  isComplete: boolean;
  isCurrent: boolean;
  href?: string;
};

export function OnboardingJourney({
  roadmap,
  currentStep,
  accountComplete,
  planComplete,
  ownerComplete,
  billingComplete,
  shopComplete,
  programmeComplete,
  accountUrl,
  planUrl,
  ownerUrl,
  billingUrl,
  shopUrl,
  programmeUrl,
  hint,
  variant = "default",
}: OnboardingJourneyProps) {
  const steps: JourneyStep[] = roadmap
    ? roadmap.steps.map((step, index) => ({
      order: index + 1,
      key: step.key,
      label: step.label,
      compactLabel: step.compactLabel ?? undefined,
      isComplete: step.isComplete,
      isCurrent: step.isCurrent,
      href: step.isNavigable ? step.href : undefined,
    }))
    : [
      { order: 1, key: "account", label: "Create account", isComplete: accountComplete, isCurrent: currentStep === "account", href: accountUrl },
      { order: 2, key: "plan", label: "Choose plan", isComplete: planComplete, isCurrent: currentStep === "plan", href: planUrl },
      { order: 3, key: "owner", label: "Owner access", compactLabel: "Owner", isComplete: ownerComplete, isCurrent: currentStep === "owner", href: ownerUrl },
      { order: 4, key: "billing", label: "Billing", compactLabel: "Billing", isComplete: billingComplete, isCurrent: currentStep === "billing", href: billingUrl },
      { order: 5, key: "shop", label: "Shop details", isComplete: shopComplete, isCurrent: currentStep === "shop", href: shopUrl },
      { order: 6, key: "programme", label: "Programme template", isComplete: programmeComplete, isCurrent: currentStep === "programme", href: programmeUrl },
    ];

  const completeCount = roadmap?.completeCount ?? steps.filter((step) => step.isComplete).length;
  const currentIndex = Math.max(0, steps.findIndex((step) => step.isCurrent));
  const mobileWindowSteps = steps.filter((_, index) => Math.abs(index - currentIndex) <= 1);
  const currentStepNumber = currentIndex + 1;

  const renderStandardRow = (step: JourneyStep) => {
    const isNavigable = Boolean(step.href && (step.isComplete || step.isCurrent));

    const body = (
      <div
        className={`flex items-center gap-3 rounded-xl border p-3 transition ${
          step.isCurrent
            ? "border-secondary/70 bg-secondary/10"
            : step.isComplete
              ? "border-[rgba(200,169,106,0.42)] bg-[rgba(200,169,106,0.08)]"
              : "border-border bg-background"
        }`}
      >
        <span
          className={`flex h-7 w-7 items-center justify-center rounded-full border text-xs font-semibold ${
            step.isCurrent
              ? "border-primary/70 bg-primary text-primary-foreground"
              : step.isComplete
                ? "border-[rgba(200,169,106,0.78)] bg-[#c8a96a] text-[#0f1b2a]"
                : "border-border bg-background text-muted-foreground"
          }`}
        >
          {step.isComplete ? <CheckCircle2 className="h-4 w-4" /> : step.order}
        </span>
        <span className="flex-1 text-sm font-medium">
          {step.compactLabel ? <span className="sm:hidden">{step.compactLabel}</span> : null}
          {step.compactLabel ? <span className="hidden sm:inline">{step.label}</span> : step.label}
        </span>
      </div>
    );

    if (isNavigable) {
      return (
        <Link href={step.href as string} className="block">
          {body}
        </Link>
      );
    }

    return body;
  };

  const renderMapNode = (step: JourneyStep, index: number, total: number) => {
    const isNavigable = Boolean(step.href && (step.isComplete || step.isCurrent));
    const isSegmentComplete = step.isComplete;

    const node = (
      <div className="relative flex flex-col items-center gap-1 text-center">
        {step.isCurrent ? (
          <span className="absolute -top-2 h-12 w-12 rounded-full bg-primary/20 blur-xl" aria-hidden />
        ) : null}
        <span
          className={`relative z-10 flex h-8 w-8 items-center justify-center rounded-full border text-[11px] font-semibold sm:h-9 sm:w-9 sm:text-xs ${
            step.isCurrent
              ? "border-primary/70 bg-primary text-primary-foreground shadow-[0_0_24px_rgba(15,27,42,0.32)]"
              : step.isComplete
                ? "border-[rgba(200,169,106,0.78)] bg-[#c8a96a] text-[#0f1b2a] shadow-[0_12px_24px_-16px_rgba(200,169,106,0.92)]"
                : "border-border bg-background text-muted-foreground"
          }`}
        >
          {step.isComplete ? <Check className="h-4 w-4" /> : step.order}
        </span>
        <div className="mt-0.5">
          <p className={`text-xs font-medium leading-4 sm:text-sm sm:leading-5 ${step.isCurrent ? "text-foreground" : step.isComplete ? "text-foreground/88" : "text-foreground/70"} ${step.isCurrent ? "" : "hidden sm:block"}`}>
            {step.compactLabel ? <span className="sm:hidden">{step.compactLabel}</span> : null}
            {step.compactLabel ? <span className="hidden sm:inline">{step.label}</span> : step.label}
          </p>
        </div>
      </div>
    );

    return (
      <li key={step.key} className="relative flex-1">
        {index < total - 1 ? (
          <span
            aria-hidden
            className="absolute left-1/2 right-[-50%] top-4 h-px bg-muted-foreground/35 sm:top-[1.25rem]"
          />
        ) : null}
        {index < total - 1 && isSegmentComplete ? (
          <span
            aria-hidden
            className="absolute left-1/2 right-[-50%] top-4 h-px bg-[linear-gradient(90deg,#0f1b2a,#6d5a34,#c8a96a)] sm:top-[1.25rem]"
            style={{ opacity: 0.92 }}
          />
        ) : null}
        {isNavigable ? (
          <Link
            href={step.href as string}
            aria-current={step.isCurrent ? "step" : undefined}
            className="block rounded-lg transition hover:scale-[1.01] hover:bg-primary/5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/40 focus-visible:ring-offset-2"
          >
            {node}
          </Link>
        ) : (
          node
        )}
      </li>
    );
  };

  return (
    <section className="glass-panel relative max-h-[12rem] space-y-1.5 overflow-hidden rounded-[1.85rem] border-[rgba(15,27,42,0.12)] bg-[linear-gradient(180deg,rgba(255,251,245,0.98),rgba(247,243,236,0.94))] p-3 shadow-[0_22px_48px_-36px_rgba(15,27,42,0.34)] sm:max-h-none sm:space-y-2 sm:p-4" aria-label="Onboarding journey">
      <div aria-hidden className="pointer-events-none absolute -left-16 -top-14 h-52 w-52 rounded-full bg-primary/8 blur-3xl" />
      <div aria-hidden className="pointer-events-none absolute -right-10 top-6 h-44 w-44 rounded-full bg-secondary/12 blur-3xl" />

      <div className="flex items-center justify-between gap-2">
        <div className="inline-flex items-center gap-1.5">
          <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">Signup roadmap</p>
        </div>
        <div className="flex items-center gap-1.5">
          <Badge className="border-[rgba(200,169,106,0.24)] bg-[rgba(200,169,106,0.12)] text-[#6f592f]">
            <span className="sm:hidden">{currentStepNumber}/{steps.length}</span>
            <span className="hidden sm:inline">Step {currentStepNumber} of {steps.length}</span>
          </Badge>
        </div>
      </div>

      {variant === "compact" ? (
        <>
          <div className="relative md:hidden">
            <div aria-hidden className="pointer-events-none absolute inset-y-0 left-0 z-10 w-4 bg-gradient-to-r from-[rgb(255,250,240)] to-transparent" />
            <div aria-hidden className="pointer-events-none absolute inset-y-0 right-0 z-10 w-4 bg-gradient-to-l from-[rgb(255,250,240)] to-transparent" />
            <ol className="flex items-start gap-2.5">
              {mobileWindowSteps.map((step, index) => renderMapNode(step, index, mobileWindowSteps.length))}
            </ol>
          </div>
          <ol className="hidden items-start gap-2.5 md:flex">
            {steps.map((step, index) => renderMapNode(step, index, steps.length))}
          </ol>
        </>
      ) : (
        <ol className="grid gap-2">
          {steps.map((step) => (
            <li key={step.key}>{renderStandardRow(step)}</li>
          ))}
        </ol>
      )}

    </section>
  );
}
