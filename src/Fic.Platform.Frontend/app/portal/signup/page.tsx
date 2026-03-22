"use client";

import { FormEvent, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { ArrowRight, CheckCircle2, Compass, ShieldCheck } from "lucide-react";
import { toast } from "sonner";
import { createMerchant } from "@/lib/api";
import { saveSignupMerchantDraft } from "@/lib/onboarding-draft";
import { useSignupPortalNavigationQuery } from "@/lib/queries";
import { northStarPortalTheme, type PortalNavigationContract } from "@/types/portal-contracts";
import { Button } from "@/components/ui/button";
import { OnboardingJourney } from "@/components/layout/onboarding-journey";
import { PortalShell } from "@/components/layout/portal-shell";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export default function SignupPage() {
  const router = useRouter();
  const [displayName, setDisplayName] = useState("");
  const [ownerName, setOwnerName] = useState("");
  const [contactEmail, setContactEmail] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const portalNavQuery = useSignupPortalNavigationQuery("signup");
  const portalNav: PortalNavigationContract | null = portalNavQuery.data ?? null;

  const canSubmit = useMemo(() => {
    return !isSubmitting
      && displayName.trim().length > 0
      && ownerName.trim().length > 0
      && contactEmail.trim().length > 0;
  }, [contactEmail, displayName, isSubmitting, ownerName]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!displayName.trim() || !ownerName.trim() || !contactEmail.trim()) {
      setError("Enter shop name, owner name, and owner email to continue.");
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const workspace = await createMerchant({
        displayName,
        contactEmail,
        ownerName,
      });
      saveSignupMerchantDraft({
        merchantId: workspace.merchant.merchantId,
        displayName: workspace.merchant.displayName,
        contactEmail: workspace.merchant.contactEmail,
        ownerName,
        shopTypeKey: workspace.merchant.shopTypeKey,
        townOrCity: workspace.merchant.townOrCity,
        postcode: workspace.merchant.postcode,
      });

      router.push(`/portal/signup/plan/${workspace.merchant.merchantId}`);
    } catch (err) {
      const rawMessage = err instanceof Error ? err.message : "Unable to create merchant right now.";
      const isConnectivityError = /load failed|failed to fetch|network/i.test(rawMessage);
      const message = (
        isConnectivityError
          ? "Unable to reach the API right now. Confirm the backend is running and try again."
          : rawMessage
      );
      setError(message);
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <PortalShell
      title="Merchant setup"
      activeKey={portalNav?.activeKey ?? "signup"}
      railItems={portalNav?.items ?? []}
      theme={northStarPortalTheme}
      utilityLinks={portalNav?.utilityLinks}
      showRail={false}
      showActiveBadge={false}
      headerMode="onboarding"
    >
      <div className="space-y-6">
        <OnboardingJourney
          roadmap={portalNav?.roadmap}
          currentStep="account"
          accountComplete={false}
          planComplete={false}
          ownerComplete={false}
          billingComplete={false}
          shopComplete={false}
          programmeComplete={false}
          accountUrl="/portal/signup"
          variant="compact"
        />

        <div className="onboarding-stage items-start">
          <div className="space-y-5">
            <section className="section-intro space-y-4">
              <div className="onboarding-kicker">
                <Compass className="h-3.5 w-3.5" />
                Merchant onboarding
              </div>
              <h1 className="luxe-title">Create your merchant account</h1>
              <p className="luxe-subtitle text-foreground/85">Set the core merchant details now, then move through plan, ownership, billing, and programme setup in one guided lane.</p>
            </section>

            <Card className="onboarding-premium-panel rounded-[2rem] border-[rgba(15,27,42,0.12)] bg-transparent">
              <CardHeader className="relative z-10 pb-5">
                <div className="flex flex-wrap items-start justify-between gap-4">
                  <div>
                    <CardTitle className="text-[2rem] text-[#0f1b2a]">Merchant setup</CardTitle>
                    <CardDescription className="mt-2 max-w-2xl text-[0.98rem] leading-7 text-[rgba(74,79,85,0.92)]">
                      Designed for a fast first completion with only the essentials required to open the workspace.
                    </CardDescription>
                  </div>
                  <div className="rounded-[1.3rem] border border-[rgba(200,169,106,0.24)] bg-[rgba(200,169,106,0.08)] px-4 py-3 text-sm text-[#6f592f]">
                    <p className="text-[11px] uppercase tracking-[0.18em]">North Star standard</p>
                    <p className="mt-1 max-w-[14rem] leading-6">Clear setup first, complexity later.</p>
                  </div>
                </div>
              </CardHeader>
              <CardContent className="relative z-10">
                <form className="space-y-5" onSubmit={handleSubmit}>
                  <div className="grid gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="displayName" className="text-[13px] uppercase tracking-[0.12em] text-[rgba(74,79,85,0.9)]">Shop name</Label>
                      <Input
                        id="displayName"
                        value={displayName}
                        onChange={(event) => setDisplayName(event.target.value)}
                        placeholder="Jo's Coffee"
                        className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.92)] text-base shadow-[inset_0_1px_0_rgba(255,255,255,0.85)] placeholder:text-[rgba(74,79,85,0.5)] focus-visible:ring-[rgba(15,27,42,0.25)]"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="ownerName" className="text-[13px] uppercase tracking-[0.12em] text-[rgba(74,79,85,0.9)]">Owner name</Label>
                      <Input
                        id="ownerName"
                        value={ownerName}
                        onChange={(event) => setOwnerName(event.target.value)}
                        placeholder="Jo Taylor"
                        className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.92)] text-base shadow-[inset_0_1px_0_rgba(255,255,255,0.85)] placeholder:text-[rgba(74,79,85,0.5)] focus-visible:ring-[rgba(15,27,42,0.25)]"
                      />
                    </div>

                    <div className="space-y-2">
                      <Label htmlFor="contactEmail" className="text-[13px] uppercase tracking-[0.12em] text-[rgba(74,79,85,0.9)]">Owner email</Label>
                      <Input
                        id="contactEmail"
                        value={contactEmail}
                        onChange={(event) => setContactEmail(event.target.value)}
                        placeholder="owner@shop.test"
                        type="email"
                        className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.92)] text-base shadow-[inset_0_1px_0_rgba(255,255,255,0.85)] placeholder:text-[rgba(74,79,85,0.5)] focus-visible:ring-[rgba(15,27,42,0.25)]"
                      />
                    </div>
                  </div>

                  <Button
                    type="submit"
                    className="h-14 w-full rounded-full border border-[rgba(200,169,106,0.34)] bg-[#0f1b2a] text-[#f5f3ef] shadow-[0_18px_38px_-24px_rgba(15,27,42,0.78)] enabled:hover:bg-[#18283a] disabled:opacity-50"
                    size="lg"
                    disabled={!canSubmit}
                  >
                    {isSubmitting ? "Creating..." : "Continue to plan"}
                    <ArrowRight className="h-4 w-4" />
                  </Button>

                  {error ? <p className="text-sm text-destructive">{error}</p> : null}
                </form>
              </CardContent>
            </Card>
          </div>

          <aside className="space-y-4">
            <section className="relative overflow-hidden rounded-[1.8rem] border border-[rgba(200,169,106,0.18)] bg-[linear-gradient(180deg,#0f1b2a,#16283a)] p-6 text-[#f5f3ef] shadow-[0_24px_60px_-40px_rgba(15,27,42,0.7)]">
              <div aria-hidden className="pointer-events-none absolute -right-10 -top-8 h-32 w-32 rounded-full bg-[rgba(200,169,106,0.12)] blur-3xl" />
              <p className="relative z-10 text-xs uppercase tracking-[0.18em] text-[rgba(200,169,106,0.86)]">What happens next</p>
              <div className="mt-4 space-y-4">
                <div className="relative z-10">
                  <p className="font-medium text-[#f5f3ef]">1. Choose your starting plan</p>
                  <p className="mt-1 text-sm leading-7 text-[rgba(245,243,239,0.74)]">Pick the self-serve route that fits your rollout pace and commercial needs.</p>
                </div>
                <div className="relative z-10">
                  <p className="font-medium text-[#f5f3ef]">2. Confirm owner access</p>
                  <p className="mt-1 text-sm leading-7 text-[rgba(245,243,239,0.74)]">Create credentials and set billing so the workspace is properly owned from day one.</p>
                </div>
                <div className="relative z-10">
                  <p className="font-medium text-[#f5f3ef]">3. Set up your first programme</p>
                  <p className="mt-1 text-sm leading-7 text-[rgba(245,243,239,0.74)]">Add shop details, choose a template, and move into operating mode.</p>
                </div>
              </div>
            </section>

            <section className="onboarding-aside-card">
              <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">North Star principles</p>
              <div className="mt-4 space-y-4">
                <div className="flex items-start gap-3">
                  <CheckCircle2 className="mt-0.5 h-4 w-4 text-[#c8a96a]" />
                  <p className="text-sm leading-7 text-muted-foreground">Keep early steps short and decisive so setup feels credible, not overwhelming.</p>
                </div>
                <div className="flex items-start gap-3">
                  <ShieldCheck className="mt-0.5 h-4 w-4 text-[#c8a96a]" />
                  <p className="text-sm leading-7 text-muted-foreground">Carry one calm visual language from entry page to account creation and onward into the workspace.</p>
                </div>
              </div>
            </section>
          </aside>
        </div>
      </div>
    </PortalShell>
  );
}
