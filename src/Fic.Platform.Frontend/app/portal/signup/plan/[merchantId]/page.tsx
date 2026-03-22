"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { ArrowRight, Check, Compass } from "lucide-react";
import { toast } from "sonner";
import { readSignupMerchantDraft, saveSignupMerchantDraft } from "@/lib/onboarding-draft";
import { useSignupPortalNavigationQuery, useWorkspaceSnapshotQuery } from "@/lib/queries";
import type { MerchantWorkspaceSnapshot } from "@/types/contracts";
import { northStarPortalTheme, type PortalNavigationContract } from "@/types/portal-contracts";
import { OnboardingJourney } from "@/components/layout/onboarding-journey";
import { PortalShell } from "@/components/layout/portal-shell";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function SignupPlanPage() {
  const router = useRouter();
  const params = useParams<{ merchantId: string }>();
  const merchantId = Array.isArray(params.merchantId) ? params.merchantId[0] : params.merchantId;

  const [draftDisplayName, setDraftDisplayName] = useState("Your shop");
  const workspaceQuery = useWorkspaceSnapshotQuery(merchantId);
  const portalNavQuery = useSignupPortalNavigationQuery("plan", merchantId);
  const workspace: MerchantWorkspaceSnapshot | null = workspaceQuery.data ?? null;
  const portalNav: PortalNavigationContract | null = portalNavQuery.data ?? null;

  useEffect(() => {
    const draft = readSignupMerchantDraft(merchantId);
    if (draft) {
      setDraftDisplayName(draft.displayName);
    }
  }, [merchantId]);

  useEffect(() => {
    if (!workspace) {
      return;
    }

    const existingDraft = readSignupMerchantDraft(merchantId);
    saveSignupMerchantDraft({
      merchantId: workspace.merchant.merchantId,
      displayName: workspace.merchant.displayName,
      contactEmail: workspace.merchant.contactEmail,
      ownerName: existingDraft?.ownerName,
      shopTypeKey: workspace.merchant.shopTypeKey,
      townOrCity: workspace.merchant.townOrCity,
      postcode: workspace.merchant.postcode,
    });
  }, [merchantId, workspace]);

  useEffect(() => {
    if (workspaceQuery.error) {
      toast.error("Workspace data could not be loaded.");
    }
  }, [workspaceQuery.error]);

  const tiers = [
    {
      key: "starter",
      name: "Starter",
      tagline: "Self-serve",
      priceLabel: "GBP 19.99/mo",
      description: "For independent merchants launching a first programme with a self-serve setup path.",
      isSelfServeEnabled: true,
      features: [
        "Programme templates and QR join flow",
        "Apple Wallet pass delivery path",
        "Owner access and daily operate tooling",
      ],
    },
    {
      key: "growth",
      name: "Growth",
      tagline: "Sales assisted",
      priceLabel: "GBP 79/mo",
      description: "For multi-site operators that need more rollout support and operating controls.",
      isSelfServeEnabled: false,
      features: [
        "Multi-location rollout planning",
        "Priority support windows",
        "Operational health reviews",
      ],
    },
    {
      key: "enterprise",
      name: "Enterprise",
      tagline: "Contact us",
      priceLabel: "Custom",
      description: "For operators that need governance, integrations, and a consultancy-led rollout.",
      isSelfServeEnabled: false,
      features: [
        "SSO and access governance",
        "CRM integrations and data handoff",
        "Consultancy-led rollout",
      ],
    },
  ] as const;

  return (
    <PortalShell
      title="Merchant setup"
      activeKey={portalNav?.activeKey ?? "plan"}
      railItems={portalNav?.items ?? []}
      theme={northStarPortalTheme}
      utilityLinks={portalNav?.utilityLinks}
      showRail={false}
      showActiveBadge={false}
      headerMode="onboarding"
    >
      <div className="onboarding-shell">
      <OnboardingJourney
        roadmap={portalNav?.roadmap}
        currentStep="plan"
        accountComplete
        planComplete={false}
        ownerComplete={false}
        billingComplete={false}
        shopComplete={false}
        programmeComplete={false}
        accountUrl="/portal/signup"
        planUrl={`/portal/signup/plan/${merchantId}`}
        variant="compact"
      />

      <section className="section-intro space-y-4">
        <div className="onboarding-kicker">
          <Compass className="h-3.5 w-3.5" />
          Plan
        </div>
        <h1 className="luxe-title">Choose a plan</h1>
        <p className="luxe-subtitle text-foreground/90">Select the starting plan for {workspace?.merchant.displayName ?? draftDisplayName}.</p>
      </section>

      <section>
        <Card className="onboarding-stage-card">
          <CardHeader>
            <CardTitle className="text-3xl">Plan selection</CardTitle>
            <CardDescription className="text-foreground/90">Starter continues directly into owner access and billing. Assisted plans move into a supported route.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid gap-4 lg:grid-cols-3">
              {tiers.map((tier) => (
                <article
                  key={tier.key}
                  className={`onboarding-choice-card flex h-full flex-col ${
                    tier.isSelfServeEnabled ? "onboarding-choice-card--active" : ""
                  }`}
                >
                  <div className="space-y-2">
                    <div className="flex items-center justify-between gap-2">
                      <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">{tier.tagline}</p>
                      {tier.isSelfServeEnabled ? <Badge className="border-[rgba(200,169,106,0.28)] bg-[rgba(200,169,106,0.16)] text-[#6f592f]">Recommended</Badge> : null}
                    </div>
                    <h3 className="font-display text-[1.9rem] leading-tight">{tier.name}</h3>
                    <p className="text-lg font-semibold">{tier.priceLabel}</p>
                    <p className="text-sm text-foreground/85">{tier.description}</p>
                  </div>

                  <ul className="mt-4 grid gap-2 text-sm leading-6 text-foreground/90">
                    {(tier.isSelfServeEnabled ? tier.features : tier.features.slice(0, 2)).map((feature) => (
                      <li key={feature} className="flex items-start gap-2">
                        <Check className="mt-0.5 h-4 w-4 text-[#c8a96a]" />
                        <span>{feature}</span>
                      </li>
                    ))}
                  </ul>

                  <div className="mt-5">
                    {tier.isSelfServeEnabled ? (
                      <Button
                        type="button"
                        className="w-full rounded-full bg-[#0f1b2a] text-[#f5f3ef] hover:bg-[#1b2d40]"
                        onClick={() => router.push(`/portal/signup/billing/${merchantId}?plan=starter`)}
                      >
                        Continue with Starter
                        <ArrowRight className="h-4 w-4" />
                      </Button>
                    ) : (
                      <Button asChild variant="outline" className="w-full rounded-full border-[rgba(15,27,42,0.14)] bg-transparent text-[#0f1b2a] hover:bg-[rgba(15,27,42,0.04)]">
                        <Link href="/consultancy">Discuss this rollout</Link>
                      </Button>
                    )}
                  </div>
                </article>
              ))}
            </div>

            <Button asChild variant="ghost">
              <Link href="/portal/signup">Back to merchant details</Link>
            </Button>
          </CardContent>
        </Card>
      </section>
      </div>
    </PortalShell>
  );
}
