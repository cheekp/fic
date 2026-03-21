"use client";

import Link from "next/link";
import Image from "next/image";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { FormEvent, useEffect, useMemo, useState } from "react";
import { CreditCard, LockKeyhole } from "lucide-react";
import { completeSignup, getSignupPortalNavigation } from "@/lib/api";
import { readSignupMerchantDraft } from "@/lib/onboarding-draft";
import { ficPortalTheme, type PortalNavigationContract } from "@/types/portal-contracts";
import { OnboardingJourney } from "@/components/layout/onboarding-journey";
import { PortalShell } from "@/components/layout/portal-shell";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export default function SignupBillingPage() {
  const router = useRouter();
  const params = useParams<{ merchantId: string }>();
  const searchParams = useSearchParams();
  const merchantId = Array.isArray(params.merchantId) ? params.merchantId[0] : params.merchantId;
  const selectedPlan = searchParams.get("plan") ?? "starter";
  const requestedStage = searchParams.get("stage") === "billing" ? "billing" : "owner";

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [paymentMethod, setPaymentMethod] = useState<"apple-pay" | "card">("apple-pay");
  const [ownerName, setOwnerName] = useState("");
  const [ownerEmail, setOwnerEmail] = useState("");
  const [ownerAccessConfirmed, setOwnerAccessConfirmed] = useState(false);
  const [billingConfirmed, setBillingConfirmed] = useState(false);
  const [cardholderName, setCardholderName] = useState("");
  const [cardNumber, setCardNumber] = useState("");
  const [cardExpiry, setCardExpiry] = useState("");
  const [cardCvc, setCardCvc] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [portalNav, setPortalNav] = useState<PortalNavigationContract | null>(null);
  const currentRoadmapStep = requestedStage;
  const isOwnerStage = currentRoadmapStep === "owner";
  const isBillingStage = currentRoadmapStep === "billing";

  useEffect(() => {
    let cancelled = false;
    getSignupPortalNavigation(currentRoadmapStep, merchantId)
      .then((next) => {
        if (!cancelled) {
          setPortalNav(next);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setPortalNav(null);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [currentRoadmapStep, merchantId]);

  useEffect(() => {
    const draft = readSignupMerchantDraft(merchantId);
    if (!draft) {
      return;
    }

    setOwnerName(draft.ownerName ?? "");
    setOwnerEmail(draft.contactEmail);
  }, [merchantId]);

  useEffect(() => {
    if (ownerAccessConfirmed) {
      setOwnerAccessConfirmed(false);
    }
    if (billingConfirmed) {
      setBillingConfirmed(false);
    }
  }, [confirmPassword, password]);

  useEffect(() => {
    if (billingConfirmed) {
      setBillingConfirmed(false);
    }
  }, [cardCvc, cardExpiry, cardNumber, cardholderName, ownerAccessConfirmed, paymentMethod]);

  const canSubmit = useMemo(
    () =>
      selectedPlan === "starter"
      && ownerAccessConfirmed
      && billingConfirmed
      && password.length >= 8
      && confirmPassword.length >= 8
      && password === confirmPassword,
    [billingConfirmed, confirmPassword.length, ownerAccessConfirmed, password, selectedPlan],
  );

  const canConfirmOwnerAccess = useMemo(
    () =>
      password.length >= 8
      && confirmPassword.length >= 8
      && password === confirmPassword,
    [confirmPassword.length, password, password.length],
  );

  const canConfirmBilling = useMemo(() => {
    if (paymentMethod === "apple-pay") {
      return true;
    }

    return cardholderName.trim().length > 0
      && cardNumber.trim().length > 0
      && cardExpiry.trim().length > 0
      && cardCvc.trim().length > 0;
  }, [cardCvc, cardExpiry, cardNumber, cardholderName, paymentMethod]);

  function handleConfirmOwnerAccess() {
    if (!canConfirmOwnerAccess) {
      setError("Use matching passwords with at least 8 characters to confirm owner access.");
      return;
    }

    setError(null);
    setOwnerAccessConfirmed(true);
    if (isOwnerStage) {
      router.replace(`/portal/signup/billing/${merchantId}?plan=${selectedPlan}&stage=billing`);
    }
  }

  function handleConfirmBillingDetails() {
    if (!ownerAccessConfirmed) {
      setError("Confirm owner access before billing.");
      return;
    }

    if (!canConfirmBilling) {
      setError("Complete payment details before continuing.");
      return;
    }

    setError(null);
    setBillingConfirmed(true);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!ownerAccessConfirmed) {
      setError("Confirm owner access before continuing.");
      return;
    }

    if (!billingConfirmed) {
      setError("Confirm billing details before continuing.");
      return;
    }

    if (password !== confirmPassword) {
      setError("Password confirmation does not match.");
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      await completeSignup({
        merchantId,
        plan: "starter",
        password,
        confirmPassword,
      });

      router.push(`/portal/merchant/${merchantId}`);
    } catch (err) {
      const message = err instanceof Error ? err.message : "Unable to complete signup.";
      setError(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <PortalShell
      title="Merchant portal"
      activeKey={portalNav?.activeKey ?? "billing"}
      railItems={portalNav?.items ?? []}
      theme={portalNav?.theme ?? ficPortalTheme}
      showRail={false}
      showActiveBadge={false}
    >
      <div className="space-y-5">
      <OnboardingJourney
        roadmap={null}
        currentStep={currentRoadmapStep}
        accountComplete
        planComplete
        ownerComplete={ownerAccessConfirmed}
        billingComplete={billingConfirmed}
        shopComplete={false}
        programmeComplete={false}
        accountUrl="/portal/signup"
        planUrl={`/portal/signup/plan/${merchantId}`}
        ownerUrl={`/portal/signup/billing/${merchantId}?plan=${selectedPlan}&stage=owner`}
        billingUrl={`/portal/signup/billing/${merchantId}?plan=${selectedPlan}&stage=billing`}
        variant="compact"
      />

      <section className="section-intro space-y-3">
        <Badge>{isOwnerStage ? "Step 3 of 6" : "Step 4 of 6"}</Badge>
        <h1 className="luxe-title">{isOwnerStage ? "Set owner access" : "Confirm billing"}</h1>
        <p className="luxe-subtitle text-foreground/90">
          {isOwnerStage
            ? `Plan selected: ${selectedPlan}. Secure owner access before payment setup.`
            : `Plan selected: ${selectedPlan}. Confirm payment details to continue to workspace.`}
        </p>
      </section>

      <section>
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-3xl">
              <LockKeyhole className="h-5 w-5 text-secondary" />
              {isOwnerStage ? "Owner security setup" : "Billing confirmation"}
            </CardTitle>
            <CardDescription className="text-foreground/90">
              {isOwnerStage
                ? "Create secure owner credentials to unlock billing."
                : "Owner access is in place. Confirm payment to finish onboarding."}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-5">
            <form className="space-y-5" onSubmit={handleSubmit}>
              <section className="glass-panel space-y-4 border-secondary/35 bg-secondary/10 p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Selected plan</p>
                    <p className="mt-1 text-lg font-semibold">Starter</p>
                    <p className="text-sm text-foreground/75">GBP 19.99/mo self-serve onboarding path.</p>
                  </div>
                  <Button asChild variant="outline" size="sm">
                    <Link href={`/portal/signup/plan/${merchantId}`}>Change plan</Link>
                  </Button>
                </div>

                {(ownerName || ownerEmail) ? (
                  <div className="grid gap-2 text-sm text-foreground/80 sm:grid-cols-2">
                    {ownerName ? <p>Owner: <span className="font-semibold text-foreground">{ownerName}</span></p> : null}
                    {ownerEmail ? <p>Email: <span className="font-semibold text-foreground">{ownerEmail}</span></p> : null}
                  </div>
                ) : null}
              </section>

              {isOwnerStage ? (
                <section className="space-y-4 rounded-2xl border border-border/70 bg-background/80 p-4">
                  <div className="space-y-1">
                    <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Owner access step</p>
                    <h3 className="text-lg font-semibold">Set owner credentials</h3>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="password">Owner password</Label>
                    <Input
                      id="password"
                      value={password}
                      onChange={(event) => setPassword(event.target.value)}
                      type="password"
                      minLength={8}
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="confirm-password">Confirm password</Label>
                    <Input
                      id="confirm-password"
                      value={confirmPassword}
                      onChange={(event) => setConfirmPassword(event.target.value)}
                      type="password"
                      minLength={8}
                    />
                  </div>

                  <div className="flex flex-wrap items-center gap-3">
                    <Button
                      type="button"
                      variant={ownerAccessConfirmed ? "secondary" : "default"}
                      onClick={handleConfirmOwnerAccess}
                    >
                      {ownerAccessConfirmed ? "Owner access confirmed" : "Confirm owner access"}
                    </Button>
                    <Button
                      type="button"
                      variant="outline"
                      onClick={() => router.replace(`/portal/signup/billing/${merchantId}?plan=${selectedPlan}&stage=billing`)}
                      disabled={!ownerAccessConfirmed}
                    >
                      Continue to billing
                    </Button>
                  </div>
                </section>
              ) : (
                <section className="space-y-3 rounded-2xl border border-border/70 bg-background/80 p-4">
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Owner access</p>
                      <p className="text-sm font-semibold text-foreground">
                        {ownerAccessConfirmed ? "Owner access confirmed" : "Owner access not yet confirmed in this session"}
                      </p>
                    </div>
                    <Button asChild variant="outline" size="sm">
                      <Link href={`/portal/signup/billing/${merchantId}?plan=${selectedPlan}&stage=owner`}>
                        Edit owner access
                      </Link>
                    </Button>
                  </div>
                  {!ownerAccessConfirmed ? (
                    <p className="text-sm text-destructive">Return to step 3 and confirm owner access before billing.</p>
                  ) : null}
                </section>
              )}

              {isBillingStage ? (
                <section className={`space-y-4 rounded-2xl border border-border/70 bg-background/80 p-4 ${ownerAccessConfirmed ? "" : "opacity-65"}`}>
                <div className="space-y-1">
                  <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Billing step</p>
                  <h3 className="text-lg font-semibold">Confirm payment details</h3>
                </div>
                <div className="grid gap-3 sm:grid-cols-2">
                  <button
                    type="button"
                    className={`glass-panel flex items-center justify-between p-4 text-left transition ${
                      paymentMethod === "apple-pay" ? "border-secondary/60 bg-secondary/10" : ""
                    }`}
                    onClick={() => setPaymentMethod("apple-pay")}
                    disabled={!ownerAccessConfirmed}
                  >
                    <span>
                      <span className="block text-sm font-semibold">Apple Pay</span>
                      <span className="text-xs text-foreground/70">Fastest setup on supported devices and browsers.</span>
                    </span>
                    <Image src="/branding/apple-pay-mark.svg" alt="Apple Pay" width={48} height={20} className="h-5 w-auto opacity-80" />
                  </button>

                  <button
                    type="button"
                    className={`glass-panel flex items-center justify-between p-4 text-left transition ${
                      paymentMethod === "card" ? "border-secondary/60 bg-secondary/10" : ""
                    }`}
                    onClick={() => setPaymentMethod("card")}
                    disabled={!ownerAccessConfirmed}
                  >
                    <span>
                      <span className="block text-sm font-semibold">Card</span>
                      <span className="text-xs text-foreground/70">Use a business debit or credit card.</span>
                    </span>
                    <CreditCard className="h-4 w-4 text-muted-foreground" />
                  </button>
                </div>
                <section className="grid gap-4 sm:grid-cols-2">
                  <div className="space-y-2 sm:col-span-2">
                    <Label htmlFor="cardholder-name">Cardholder name</Label>
                    <Input
                      id="cardholder-name"
                      value={cardholderName}
                      onChange={(event) => setCardholderName(event.target.value)}
                      placeholder={ownerName || "Name on card"}
                      disabled={!ownerAccessConfirmed}
                    />
                  </div>
                  <div className="space-y-2 sm:col-span-2">
                    <Label htmlFor="card-number">Card number</Label>
                    <Input
                      id="card-number"
                      value={cardNumber}
                      onChange={(event) => setCardNumber(event.target.value)}
                      inputMode="numeric"
                      placeholder="1234 5678 9012 3456"
                      disabled={!ownerAccessConfirmed}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="card-expiry">Expiry</Label>
                    <Input
                      id="card-expiry"
                      value={cardExpiry}
                      onChange={(event) => setCardExpiry(event.target.value)}
                      inputMode="numeric"
                      placeholder="MM/YY"
                      disabled={!ownerAccessConfirmed}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="card-cvc">CVC</Label>
                    <Input
                      id="card-cvc"
                      value={cardCvc}
                      onChange={(event) => setCardCvc(event.target.value)}
                      inputMode="numeric"
                      placeholder="123"
                      disabled={!ownerAccessConfirmed}
                    />
                  </div>
                </section>

                <Button
                  type="button"
                  variant={billingConfirmed ? "secondary" : "default"}
                  onClick={handleConfirmBillingDetails}
                  disabled={!ownerAccessConfirmed}
                >
                  {billingConfirmed ? "Billing confirmed" : "Confirm billing details"}
                </Button>
              </section>
              ) : null}

              {isBillingStage ? (
                <Button type="submit" className="w-full" size="lg" disabled={!canSubmit || isSubmitting}>
                  {isSubmitting ? "Finishing setup..." : "Continue to workspace"}
                </Button>
              ) : null}
            </form>

            {error ? <p className="text-sm text-destructive">{error}</p> : null}
            <Button asChild variant="ghost">
              <Link href={`/portal/signup/plan/${merchantId}`}>Back to plan</Link>
            </Button>
          </CardContent>
        </Card>
      </section>
      </div>
    </PortalShell>
  );
}
