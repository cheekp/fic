"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { Check, Sparkles } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";

export type SetupRoadmapStep = {
  order: number;
  label: string;
  statusLabel: string;
  isComplete: boolean;
  href?: string;
};

type SetupRoadmapDialogProps = {
  headline: string;
  message?: string;
  steps: SetupRoadmapStep[];
  children?: React.ReactNode;
};

export function SetupRoadmapDialog({ headline, message, steps, children }: SetupRoadmapDialogProps) {
  const total = steps.length;
  const completed = useMemo(() => steps.filter((step) => step.isComplete).length, [steps]);
  const nextStep = steps.find((step) => !step.isComplete);

  const [isOpen, setIsOpen] = useState(false);

  if (total === 0) {
    return null;
  }

  return (
    <div className="glass-panel p-3 sm:p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="space-y-0.5">
          <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Setup workflow</p>
          <p className="text-sm font-medium text-foreground/90">
            {nextStep ? `Next: ${nextStep.label}` : "All setup tasks complete"}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Badge>{completed}/{total} complete</Badge>
          <Dialog open={isOpen} onOpenChange={setIsOpen}>
            <DialogTrigger asChild>
              <Button variant="outline" size="sm">Open roadmap</Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle className="flex items-center gap-2">
                  <Sparkles className="h-4 w-4 text-secondary" />
                  {headline}
                </DialogTitle>
              </DialogHeader>
              {message ? <p className="text-sm text-muted-foreground">{message}</p> : null}

              <div className="grid gap-2">
                {steps.map((step) => {
                  const row = (
                    <div
                      className={`flex items-center justify-between gap-3 rounded-xl border px-3 py-2.5 transition ${
                        step.isComplete ? "border-primary/40 bg-primary/5" : "border-border bg-background"
                      }`}
                    >
                      <div className="inline-flex items-center gap-2.5">
                        <span
                          className={`inline-flex h-6 w-6 items-center justify-center rounded-full border text-xs font-semibold ${
                            step.isComplete
                              ? "border-primary/60 bg-primary/15 text-primary"
                              : "border-border bg-background text-muted-foreground"
                          }`}
                        >
                          {step.isComplete ? <Check className="h-3.5 w-3.5" /> : step.order}
                        </span>
                        <p className="text-sm font-medium">{step.label}</p>
                      </div>
                      <p className="text-xs text-muted-foreground">{step.statusLabel}</p>
                    </div>
                  );

                  return step.href ? (
                    <Link key={step.order} href={step.href} onClick={() => setIsOpen(false)}>
                      {row}
                    </Link>
                  ) : (
                    <div key={step.order}>{row}</div>
                  );
                })}
              </div>

              {children}
            </DialogContent>
          </Dialog>
        </div>
      </div>
    </div>
  );
}
