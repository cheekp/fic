"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { ArrowRight } from "lucide-react";
import { createMerchant, getShopTypes } from "@/lib/api";
import { saveSignupMerchantDraft } from "@/lib/onboarding-draft";
import type { ShopTypeOption } from "@/types/contracts";
import { Button } from "@/components/ui/button";
import { OnboardingJourney } from "@/components/layout/onboarding-journey";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

export default function SignupPage() {
  const fallbackShopTypes: ShopTypeOption[] = [
    {
      shopTypeKey: "coffee",
      shopTypeLabel: "Coffee shop",
      description: "Independent and chain coffee operations.",
      isActive: true,
    },
    {
      shopTypeKey: "barber",
      shopTypeLabel: "Barbershop",
      description: "Service-led grooming and repeat booking businesses.",
      isActive: true,
    },
  ];

  const router = useRouter();
  const [shopTypes, setShopTypes] = useState<ShopTypeOption[]>([]);
  const [displayName, setDisplayName] = useState("");
  const [contactEmail, setContactEmail] = useState("");
  const [shopTypeKey, setShopTypeKey] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loadNotice, setLoadNotice] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;

    getShopTypes()
      .then((types) => {
        if (cancelled) {
          return;
        }

        const active = types.filter((type) => type.isActive);
        const effective = active.length > 0 ? active : fallbackShopTypes;
        setShopTypes(effective);
        setShopTypeKey((current) => current || effective[0]?.shopTypeKey || "");
        setLoadNotice(active.length > 0 ? null : "Live catalogue returned no active shop types. Using defaults.");
      })
      .catch((err: Error) => {
        if (!cancelled) {
          setShopTypes(fallbackShopTypes);
          setShopTypeKey((current) => current || fallbackShopTypes[0].shopTypeKey);
          setLoadNotice("Catalogue unavailable. Using default shop types.");
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  const canSubmit = useMemo(() => {
    const effectiveShopTypeKey = shopTypeKey || shopTypes[0]?.shopTypeKey || "";
    return !isSubmitting && displayName.trim().length > 0 && contactEmail.trim().length > 0 && effectiveShopTypeKey.length > 0;
  }, [contactEmail, displayName, isSubmitting, shopTypeKey, shopTypes]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const effectiveShopTypeKey = shopTypeKey || shopTypes[0]?.shopTypeKey || "";
    if (!displayName.trim() || !contactEmail.trim() || !effectiveShopTypeKey) {
      setError("Enter shop name, owner email, and shop type to continue.");
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const workspace = await createMerchant({
        displayName,
        contactEmail,
        shopTypeKey: effectiveShopTypeKey,
      });
      saveSignupMerchantDraft({
        merchantId: workspace.merchant.merchantId,
        displayName: workspace.merchant.displayName,
        contactEmail: workspace.merchant.contactEmail,
        shopTypeKey: workspace.merchant.shopTypeKey,
        townOrCity: workspace.merchant.townOrCity,
        postcode: workspace.merchant.postcode,
      });

      router.push(`/portal/signup/plan/${workspace.merchant.merchantId}`);
    } catch (err) {
      const rawMessage = err instanceof Error ? err.message : "Unable to create merchant right now.";
      const isConnectivityError = /load failed|failed to fetch|network/i.test(rawMessage);
      setError(
        isConnectivityError
          ? "Unable to reach the API right now. Confirm the backend is running and try again."
          : rawMessage,
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="space-y-5">
      <OnboardingJourney
        currentStep="account"
        accountComplete={false}
        planComplete={false}
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
                <Label htmlFor="contactEmail">Owner email</Label>
                <Input
                  id="contactEmail"
                  value={contactEmail}
                  onChange={(event) => setContactEmail(event.target.value)}
                  placeholder="owner@shop.test"
                  type="email"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="shopType">Shop type</Label>
                <Select value={shopTypeKey} onValueChange={setShopTypeKey}>
                  <SelectTrigger id="shopType">
                    <SelectValue placeholder="Choose shop type" />
                  </SelectTrigger>
                  <SelectContent>
                    {shopTypes.map((type) => (
                      <SelectItem key={type.shopTypeKey} value={type.shopTypeKey}>
                        {type.shopTypeLabel}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
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

            {loadNotice ? <p className="text-xs text-muted-foreground">{loadNotice}</p> : null}
            {error ? <p className="text-sm text-destructive">{error}</p> : null}
          </form>
        </CardContent>
      </Card>
    </main>
  );
}
