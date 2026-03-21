import { ReactNode } from "react";
import { cn } from "@/lib/utils";

export function PortalHero({ className, children }: { className?: string; children: ReactNode }) {
  return <section className={cn("workspace-hero space-y-4", className)}>{children}</section>;
}

export function PortalMetricStrip({ className, children }: { className?: string; children: ReactNode }) {
  return <div className={cn("grid gap-3 sm:grid-cols-2 lg:grid-cols-4", className)}>{children}</div>;
}

export function PortalCardStack({ className, children }: { className?: string; children: ReactNode }) {
  return <section className={cn("space-y-4", className)}>{children}</section>;
}
