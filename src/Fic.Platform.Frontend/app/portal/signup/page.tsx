"use client";

import { FormEvent, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { ArrowRight } from "lucide-react";
import { toast } from "sonner";
import { createMerchant } from "@/lib/api";
import { saveSignupMerchantDraft } from "@/lib/onboarding-draft";
import { useSignupPortalNavigationQuery } from "@/lib/queries";
import { ficPortalTheme, type PortalNavigationContract } from "@/types/portal-contracts";
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
      title="Merchant portal"
      activeKey={portalNav?.activeKey ?? "signup"}
      railItems={portalNav?.items ?? []}
      theme={portalNav?.theme ?? ficPortalTheme}
      utilityLinks={portalNav?.utilityLinks}
      showRail={false}
      showActiveBadge={false}
    >
      <div className="space-y-5">
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

        <section className="section-intro space-y-3">
          <h1 className="luxe-title">Create your merchant account</h1>
          <p className="luxe-subtitle text-foreground/85">Set the essentials now. Plan and owner access continue next.</p>
        </section>

        <Card>
          <CardHeader>
            <CardTitle className="text-2xl">Merchant setup</CardTitle>
            <CardDescription>Designed for fast mobile completion with only required fields.</CardDescription>
          </CardHeader>
          <CardContent>
            <form className="space-y-5" onSubmit={handleSubmit}>
            <div className="grid gap-4">
              <div className="space-y-2">
                <Label htmlFor="displayName">Shop name</Label>
                <Input
                  id="displayName"
                  value={displayName}
                  onChange={(event) => setDisplayName(event.target.value)}
                  placeholder="Jo's Coffee"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="ownerName">Owner name</Label>
                <Input
                  id="ownerName"
                  value={ownerName}
                  onChange={(event) => setOwnerName(event.target.value)}
                  placeholder="Jo Taylor"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="contactEmail">Owner email</Label>
                <Input
                  id="contactEmail"
                  value={contactEmail}
                  onChange={(event) => setContactEmail(event.target.value)}
                  placeholder="owner@shop.test"
                  type="email"
                />
              </div>
            </div>

            <Button
              type="submit"
              className="w-full enabled:bg-primary enabled:text-primary-foreground enabled:hover:bg-primary/92"
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
    </PortalShell>
  );
}
