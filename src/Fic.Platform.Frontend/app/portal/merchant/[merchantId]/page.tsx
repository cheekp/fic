"use client";

import Link from "next/link";
import Image from "next/image";
import { useParams, useSearchParams } from "next/navigation";
import { FormEvent, useEffect, useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Copy, Eye, ExternalLink } from "lucide-react";
import { toast } from "sonner";
import { resolvePortalBrandTheme } from "@/lib/brand";
import {
  awardVisit,
  createProgramme,
  getProgrammeTemplates,
  getWorkspaceForProgramme,
  joinProgramme,
  redeemReward,
  updateCardLifecycle,
  updateCardsLifecycleBulk,
  updateMerchantBrand,
  updateProgramme,
  uploadMerchantLogo,
} from "@/lib/api";
import {
  queryKeys,
  useSessionSummaryQuery,
  useShopTypesQuery,
  useWorkspacePortalNavigationQuery,
  useWorkspaceSnapshotQuery,
} from "@/lib/queries";
import { OnboardingJourney } from "@/components/layout/onboarding-journey";
import { PortalShell } from "@/components/layout/portal-shell";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { northStarPortalTheme } from "@/types/portal-contracts";
import type {
  MerchantWorkspaceSnapshot,
  ProgrammeTemplateOption,
  ShopTypeOption,
  WalletCardSnapshot,
} from "@/types/contracts";

type WorkspaceSection = "operate" | "configure" | "customers";
type CardStatusFilter = "all" | "ready" | "active" | "redeemed" | "scheduled" | "expired" | "suspended" | "archived";

const apiBaseUrl = process.env.NEXT_PUBLIC_FIC_API_BASE_URL ?? "http://localhost:5276";

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

function resolveSection(value: string | null): WorkspaceSection {
  if (value === "configure" || value === "customers") {
    return value;
  }

  return "operate";
}

function toDateValue(value: string) {
  return value.slice(0, 10);
}

function withCacheBust(url: string, token: number) {
  if (!url || url.startsWith("data:")) {
    return url;
  }

  const separator = url.includes("?") ? "&" : "?";
  return `${url}${separator}v=${token}`;
}

function formatDateLabel(value: string) {
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return "N/A";
  }

  return parsed.toLocaleDateString(undefined, {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
}

function getCardStatusClass(card: WalletCardSnapshot) {
  if (card.canRedeem) {
    return "border-emerald-600/30 bg-emerald-600/10 text-emerald-900";
  }

  if (card.customerCardStatusLabel.toLowerCase().includes("redeemed")) {
    return "border-primary/30 bg-primary/10 text-primary";
  }

  if (card.customerCardStatusLabel.toLowerCase().includes("expired")) {
    return "border-border bg-muted text-muted-foreground";
  }

  return "border-border bg-background text-foreground/80";
}

function getCardStatusFilter(card: WalletCardSnapshot): Exclude<CardStatusFilter, "all"> {
  const label = card.customerCardStatusLabel.toLowerCase();
  if (card.canRedeem || label.includes("reward ready")) {
    return "ready";
  }

  if (label.includes("redeemed")) {
    return "redeemed";
  }

  if (label.includes("scheduled")) {
    return "scheduled";
  }

  if (label.includes("expired")) {
    return "expired";
  }

  if (label.includes("suspended")) {
    return "suspended";
  }

  if (label.includes("archived")) {
    return "archived";
  }

  return "active";
}

export default function WorkspacePage() {
  const params = useParams<{ merchantId: string }>();
  const searchParams = useSearchParams();
  const merchantId = Array.isArray(params.merchantId) ? params.merchantId[0] : params.merchantId;
  const section = resolveSection(searchParams.get("programmeSection"));
  const requestedProgrammeId = searchParams.get("programme") ?? undefined;

  const [workspace, setWorkspace] = useState<MerchantWorkspaceSnapshot | null>(null);
  const [templates, setTemplates] = useState<ProgrammeTemplateOption[]>([]);
  const [shopTypes, setShopTypes] = useState<ShopTypeOption[]>(fallbackShopTypes);
  const [selectedTemplateKey, setSelectedTemplateKey] = useState<string>("");
  const [isLoading, setIsLoading] = useState(true);
  const [isMutating, setIsMutating] = useState(false);
  const [isSavingShopDetails, setIsSavingShopDetails] = useState(false);
  const [isUploadingLogo, setIsUploadingLogo] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [scanCode, setScanCode] = useState("");
  const [cardFilter, setCardFilter] = useState("");
  const [cardStatusFilter, setCardStatusFilter] = useState<CardStatusFilter>("all");
  const [selectedCardId, setSelectedCardId] = useState<string | null>(null);
  const [isCardDetailOpen, setIsCardDetailOpen] = useState(false);
  const [selectedCardIds, setSelectedCardIds] = useState<string[]>([]);
  const [rewardItemLabel, setRewardItemLabel] = useState("");
  const [rewardThreshold, setRewardThreshold] = useState("1");
  const [rewardCopy, setRewardCopy] = useState("");
  const [startsOn, setStartsOn] = useState("");
  const [endsOn, setEndsOn] = useState("");
  const [shopTownOrCity, setShopTownOrCity] = useState("");
  const [shopPostcode, setShopPostcode] = useState("");
  const [shopTypeKey, setShopTypeKey] = useState("");
  const [isShopDraftDirty, setIsShopDraftDirty] = useState(false);
  const [brandLogoFile, setBrandLogoFile] = useState<File | null>(null);
  const [logoCacheBuster, setLogoCacheBuster] = useState(0);
  const [isShopSetupOpen, setIsShopSetupOpen] = useState(false);
  const [isSetupIntentHandled, setIsSetupIntentHandled] = useState(false);
  const queryClient = useQueryClient();

  const selectedProgramme = workspace?.selectedProgramme ?? null;
  const selectedProgrammeId = selectedProgramme?.programmeId;
  const sessionQuery = useSessionSummaryQuery();
  const workspaceQuery = useWorkspaceSnapshotQuery(merchantId, requestedProgrammeId);
  const shopTypesQuery = useShopTypesQuery();
  const portalNavQuery = useWorkspacePortalNavigationQuery(merchantId, section, selectedProgrammeId);
  const portalNav = portalNavQuery.data ?? null;
  const workspaceTheme = useMemo(
    () => resolvePortalBrandTheme({
      primaryColor: workspace?.brandProfile.primaryColor ?? northStarPortalTheme.primary,
      accentColor: workspace?.brandProfile.accentColor ?? northStarPortalTheme.accent,
    }),
    [workspace?.brandProfile.accentColor, workspace?.brandProfile.primaryColor],
  );
  const brandLogoUrl = workspace?.brandProfile.logoUrl
    ? withCacheBust(workspace.brandProfile.logoUrl, logoCacheBuster)
    : null;

  useEffect(() => {
    if (sessionQuery.isPending || workspaceQuery.isPending) {
      setIsLoading(true);
      return;
    }

    if (sessionQuery.data && !sessionQuery.data.isAuthenticated) {
      setWorkspace(null);
      setShopTypes(fallbackShopTypes);
      setError("Merchant session is not authenticated. Complete signup first.");
      setIsLoading(false);
      return;
    }

    if (workspaceQuery.error) {
      setWorkspace(null);
      setShopTypes(fallbackShopTypes);
      setError(workspaceQuery.error instanceof Error ? workspaceQuery.error.message : "Failed to load workspace.");
      setIsLoading(false);
      return;
    }

    if (workspaceQuery.data) {
      setWorkspace(workspaceQuery.data);
      setError(null);
      setIsLoading(false);
    }
  }, [
    sessionQuery.data,
    sessionQuery.isPending,
    workspaceQuery.data,
    workspaceQuery.error,
    workspaceQuery.isPending,
  ]);

  useEffect(() => {
    const activeShopTypes = (shopTypesQuery.data ?? []).filter((type) => type.isActive);
    setShopTypes(activeShopTypes.length > 0 ? activeShopTypes : fallbackShopTypes);
  }, [shopTypesQuery.data]);

  useEffect(() => {
    let cancelled = false;

    async function loadTemplates() {
      if (!workspace?.merchant.shopTypeKey) {
        setTemplates([]);
        setSelectedTemplateKey("");
        return;
      }

      try {
        const nextTemplates = await getProgrammeTemplates(workspace.merchant.shopTypeKey);
        if (cancelled) {
          return;
        }

        const activeTemplates = nextTemplates.filter((template) => template.isActive);
        setTemplates(activeTemplates);
        setSelectedTemplateKey((current) => (
          activeTemplates.some((template) => template.templateKey === current)
            ? current
            : (activeTemplates[0]?.templateKey ?? "")
        ));
      } catch {
        if (!cancelled) {
          setTemplates([]);
          setSelectedTemplateKey("");
        }
      }
    }

    loadTemplates();

    return () => {
      cancelled = true;
    };
  }, [workspace?.merchant.shopTypeKey]);

  useEffect(() => {
    if (!selectedProgramme) {
      return;
    }

    setRewardItemLabel(selectedProgramme.rewardItemLabel);
    setRewardThreshold(String(selectedProgramme.rewardThreshold));
    setRewardCopy(selectedProgramme.rewardCopy);
    setStartsOn(toDateValue(selectedProgramme.startsOn));
    setEndsOn(toDateValue(selectedProgramme.endsOn));
  }, [selectedProgramme]);

  useEffect(() => {
    if (!workspace) {
      return;
    }

    if (isShopDraftDirty) {
      return;
    }

    setShopTownOrCity(workspace.merchant.townOrCity ?? "");
    setShopPostcode(workspace.merchant.postcode ?? "");
    const existingShopType = workspace.merchant.shopTypeKey?.trim() ?? "";
    if (existingShopType.length > 0) {
      setShopTypeKey(existingShopType);
      return;
    }

    const fallbackShopType = shopTypes.find((type) => type.isActive)?.shopTypeKey ?? "";
    setShopTypeKey(fallbackShopType);
  }, [isShopDraftDirty, shopTypes, workspace]);

  useEffect(() => {
    if (!workspace || isSetupIntentHandled) {
      return;
    }

    setIsSetupIntentHandled(true);
  }, [isSetupIntentHandled, workspace]);

  useEffect(() => {
    if (portalNavQuery.error) {
      toast.error("Portal navigation unavailable.");
    }
  }, [portalNavQuery.error]);

  useEffect(() => {
    setSelectedCardIds((current) =>
      current.filter((cardId) => workspace?.selectedProgrammeCards.some((card) => card.cardId === cardId)),
    );
  }, [workspace?.selectedProgrammeCards]);

  async function refreshWorkspace(programmeId?: string) {
    const nextWorkspace = await getWorkspaceForProgramme(merchantId, programmeId);
    setWorkspace(nextWorkspace);
    queryClient.setQueryData(queryKeys.workspace(merchantId, programmeId), nextWorkspace);
    queryClient.invalidateQueries({ queryKey: ["portal-nav", "workspace", merchantId] });
  }

  const canUpdateProgramme = useMemo(
    () => selectedProgramme && rewardItemLabel.trim().length > 0 && Number(rewardThreshold) > 0,
    [rewardItemLabel, rewardThreshold, selectedProgramme],
  );

  function publishSuccess(text: string) {
    setMessage(text);
    toast.success(text);
  }

  function publishError(text: string) {
    setError(text);
    toast.error(text);
  }

  async function handleCopyCardCode(code: string) {
    try {
      await navigator.clipboard.writeText(code);
      toast.success("Card code copied.");
    } catch {
      toast.error("Unable to copy card code.");
    }
  }

  function handleOpenCardDetail(cardId: string) {
    setSelectedCardId(cardId);
    setIsCardDetailOpen(true);
  }

  async function handleCreateProgramme() {
    if (!workspace?.setupChecklist.shopDetailsComplete) {
      publishError("Complete shop details before creating the first programme.");
      return;
    }

    if (!selectedTemplateKey) {
      publishError("Select a programme template first.");
      return;
    }

    setIsMutating(true);
    setError(null);
    setMessage(null);

    try {
      const nextWorkspace = await createProgramme(merchantId, selectedTemplateKey);
      setWorkspace(nextWorkspace);
      queryClient.setQueryData(queryKeys.workspace(merchantId, requestedProgrammeId), nextWorkspace);
      queryClient.invalidateQueries({ queryKey: ["portal-nav", "workspace", merchantId] });
      publishSuccess("Programme created.");
    } catch (err) {
      publishError(err instanceof Error ? err.message : "Unable to create programme.");
    } finally {
      setIsMutating(false);
    }
  }

  async function handleUpdateProgramme(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedProgramme) {
      publishError("Select a programme before saving.");
      return;
    }

    setIsMutating(true);
    setError(null);
    setMessage(null);

    try {
      await updateProgramme(merchantId, selectedProgramme.programmeId, {
        rewardItemLabel,
        rewardThreshold: Number(rewardThreshold),
        rewardCopy,
        startsOn,
        endsOn,
      });
      await refreshWorkspace(selectedProgramme.programmeId);
      publishSuccess("Programme updated.");
    } catch (err) {
      publishError(err instanceof Error ? err.message : "Unable to update programme.");
    } finally {
      setIsMutating(false);
    }
  }

  async function handleAwardVisit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedProgramme) {
      publishError("Select a programme before awarding visits.");
      return;
    }

    setIsMutating(true);
    setError(null);
    setMessage(null);

    try {
      await awardVisit(merchantId, selectedProgramme.programmeId, scanCode.trim());
      await refreshWorkspace(selectedProgramme.programmeId);
      setScanCode("");
      publishSuccess("Visit awarded.");
    } catch (err) {
      publishError(err instanceof Error ? err.message : "Unable to award visit.");
    } finally {
      setIsMutating(false);
    }
  }

  async function handleDemoJoin() {
    if (!selectedProgramme?.joinCode) {
      publishError("Select a programme with a join code first.");
      return;
    }

    setIsMutating(true);
    setError(null);
    setMessage(null);

    try {
      const card = await joinProgramme(selectedProgramme.joinCode);
      await refreshWorkspace(selectedProgramme.programmeId);
      publishSuccess(`Customer joined with card ${card.cardCode}.`);
    } catch (err) {
      publishError(err instanceof Error ? err.message : "Unable to create demo join.");
    } finally {
      setIsMutating(false);
    }
  }

  async function handleRedeem(cardId: string) {
    if (!selectedProgramme) {
      publishError("Select a programme before redeeming.");
      return;
    }

    setIsMutating(true);
    setError(null);
    setMessage(null);

    try {
      await redeemReward(merchantId, selectedProgramme.programmeId, cardId);
      await refreshWorkspace(selectedProgramme.programmeId);
      publishSuccess("Reward redeemed.");
    } catch (err) {
      publishError(err instanceof Error ? err.message : "Unable to redeem reward.");
    } finally {
      setIsMutating(false);
    }
  }

  async function handleLifecycleAction(cardId: string, action: "suspend" | "reactivate" | "archive") {
    if (!selectedProgramme) {
      publishError("Select a programme before updating cards.");
      return;
    }

    setIsMutating(true);
    setError(null);
    setMessage(null);

    try {
      await updateCardLifecycle(merchantId, selectedProgramme.programmeId, cardId, action);
      await refreshWorkspace(selectedProgramme.programmeId);
      publishSuccess(`Card ${action}d.`);
    } catch (err) {
      publishError(err instanceof Error ? err.message : "Unable to update card lifecycle.");
    } finally {
      setIsMutating(false);
    }
  }

  async function handleBulkLifecycleAction(action: "suspend" | "reactivate" | "archive") {
    if (!selectedProgramme) {
      publishError("Select a programme before updating cards.");
      return;
    }

    if (selectedCardIds.length === 0) {
      publishError("Select one or more cards first.");
      return;
    }

    setIsMutating(true);
    setError(null);
    setMessage(null);

    try {
      await updateCardsLifecycleBulk(merchantId, selectedProgramme.programmeId, selectedCardIds, action);
      await refreshWorkspace(selectedProgramme.programmeId);
      publishSuccess(`${selectedCardIds.length} cards ${action}d.`);
      setSelectedCardIds([]);
    } catch (err) {
      publishError(err instanceof Error ? err.message : "Unable to update card lifecycle in bulk.");
    } finally {
      setIsMutating(false);
    }
  }

  async function submitShopDetails() {
    if (!workspace) {
      return;
    }

    if (!shopTypeKey.trim()) {
      publishError("Choose a shop type to continue.");
      return;
    }

    if (!shopTownOrCity.trim()) {
      publishError("Enter the town or city to continue.");
      return;
    }

    if (!shopPostcode.trim()) {
      publishError("Enter the postcode to continue.");
      return;
    }

    setIsSavingShopDetails(true);
    setError(null);
    setMessage(null);

    try {
      const nextWorkspace = await updateMerchantBrand(merchantId, {
        displayName: workspace.merchant.displayName,
        townOrCity: shopTownOrCity,
        postcode: shopPostcode,
        contactEmail: workspace.merchant.contactEmail,
        shopTypeKey,
        primaryColor: workspace.brandProfile.primaryColor,
        accentColor: workspace.brandProfile.accentColor,
        selectedProgrammeId: selectedProgramme?.programmeId,
      });

      const nextTemplates = await getProgrammeTemplates(nextWorkspace.merchant.shopTypeKey);
      const activeTemplates = nextTemplates.filter((template) => template.isActive);

      setWorkspace(nextWorkspace);
      setTemplates(activeTemplates);
      setSelectedTemplateKey(activeTemplates[0]?.templateKey ?? "");
      setIsShopDraftDirty(false);
      setIsShopSetupOpen(false);
      queryClient.setQueryData(queryKeys.workspace(merchantId, requestedProgrammeId), nextWorkspace);
      queryClient.invalidateQueries({ queryKey: ["portal-nav", "workspace", merchantId] });
      publishSuccess("Shop details saved. Next: choose your programme template.");
    } catch (err) {
      publishError(err instanceof Error ? err.message : "Unable to update shop details.");
    } finally {
      setIsSavingShopDetails(false);
    }
  }

  async function handleSaveShopDetails(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await submitShopDetails();
  }

  async function handleUploadLogo(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!brandLogoFile) {
      publishError("Select a PNG logo file before uploading.");
      return;
    }

    setIsUploadingLogo(true);
    setError(null);
    setMessage(null);

    try {
      const nextWorkspace = await uploadMerchantLogo(merchantId, brandLogoFile, selectedProgramme?.programmeId);
      setWorkspace(nextWorkspace);
      setBrandLogoFile(null);
      setLogoCacheBuster(Date.now());
      queryClient.setQueryData(queryKeys.workspace(merchantId, requestedProgrammeId), nextWorkspace);
      publishSuccess("Brand logo updated.");
    } catch (err) {
      publishError(err instanceof Error ? err.message : "Unable to upload logo.");
    } finally {
      setIsUploadingLogo(false);
    }
  }

  if (isLoading) {
    return (
      <PortalShell
        title="Merchant portal"
        activeKey={portalNav?.activeKey ?? "operate"}
        railItems={portalNav?.items ?? []}
        theme={workspaceTheme}
        utilityLinks={portalNav?.utilityLinks}
        showRail={false}
        showActiveBadge={false}
      >
        <Card>
          <CardContent className="p-6">Loading workspace...</CardContent>
        </Card>
      </PortalShell>
    );
  }

  if (!workspace) {
    return (
      <PortalShell
        title="Merchant portal"
        activeKey={portalNav?.activeKey ?? "operate"}
        railItems={portalNav?.items ?? []}
        theme={workspaceTheme}
        utilityLinks={portalNav?.utilityLinks}
        showRail={false}
        showActiveBadge={false}
      >
        <Card>
          <CardHeader>
            <CardTitle>Workspace unavailable</CardTitle>
            <CardDescription>{error ?? "No workspace payload was returned."}</CardDescription>
          </CardHeader>
          <CardContent>
            <Button asChild>
              <Link href="/portal/signup">Create and sign in a merchant</Link>
            </Button>
          </CardContent>
        </Card>
      </PortalShell>
    );
  }

  const contractNextAction = portalNav?.nextAction ?? null;
  const onboardingIncompleteFallback = !workspace.setupChecklist.shopDetailsComplete || !workspace.setupChecklist.hasAnyProgramme;
  const onboardingIncompleteFromContract = Boolean(contractNextAction);
  const shouldShowOnboarding = onboardingIncompleteFromContract || onboardingIncompleteFallback;
  const setupLaneTasks = contractNextAction?.tasks ?? [
    {
      key: "shop",
      label: "Shop details",
      isComplete: workspace.setupChecklist.shopDetailsComplete,
      isBlocked: false,
      blockedReason: null,
    },
    {
      key: "programme",
      label: "Programme template",
      isComplete: workspace.setupChecklist.hasAnyProgramme,
      isBlocked: !workspace.setupChecklist.shopDetailsComplete,
      blockedReason: !workspace.setupChecklist.shopDetailsComplete ? "Requires shop details first." : null,
    },
  ];
  const setupLaneActionKey = contractNextAction?.key ?? (workspace.setupChecklist.shopDetailsComplete ? "programme" : "shop");
  const setupLaneSummary =
    contractNextAction?.summary
    ?? (!workspace.setupChecklist.shopDetailsComplete
      ? "Add the shop type, location, and logo before creating the first programme."
      : "Choose the first programme template.");
  const setupLaneCtaLabel =
    contractNextAction?.ctaLabel
    ?? (setupLaneActionKey === "shop" ? "Open shop setup" : "Create programme");
  const setupLaneTitle = setupLaneActionKey === "shop" ? "Complete shop details" : "Choose a programme template";
  const setupLaneStatus = setupLaneTasks.filter((task) => task.isComplete).length;
  const onboardingCurrentStep =
    !workspace.setupChecklist.ownerAccessConfigured
      ? "owner"
      : !workspace.setupChecklist.shopDetailsComplete
        ? "shop"
        : "programme";
  const shopUrl = "#setup-lane";
  const programmeUrl = selectedProgramme
    ? `?programmeSection=operate&programme=${selectedProgramme.programmeId}`
    : "?programmeSection=operate";
  const filteredCards = workspace.selectedProgrammeCards
    .filter((card) => {
      const term = cardFilter.trim().toLowerCase();
      if (!term) {
        return true;
      }

      return (
        card.cardCode.toLowerCase().includes(term)
        || card.customerCardStatusLabel.toLowerCase().includes(term)
        || card.rewardItemLabel.toLowerCase().includes(term)
        || card.progressDisplayText.toLowerCase().includes(term)
      );
    })
    .filter((card) => {
      if (cardStatusFilter === "all") {
        return true;
      }

      return getCardStatusFilter(card) === cardStatusFilter;
    })
    .sort((left, right) => {
      const leftTime = new Date(left.lastUpdatedUtc).getTime();
      const rightTime = new Date(right.lastUpdatedUtc).getTime();
      return rightTime - leftTime;
    });
  const redeemableCardsCount = workspace.selectedProgrammeCards.filter((card) => card.canRedeem).length;
  const selectedCardsCount = selectedCardIds.length;
  const selectedCard = selectedCardId
    ? workspace.selectedProgrammeCards.find((card) => card.cardId === selectedCardId) ?? null
    : null;
  const selectedCardTimeline = selectedCard
    ? workspace.timeline
      .filter((event) => event.summary.toLowerCase().includes(selectedCard.cardCode.toLowerCase()))
      .slice(0, 6)
    : [];

  if (shouldShowOnboarding && section === "operate") {
    return (
      <PortalShell
        title="Merchant portal"
        activeKey="operate"
        railItems={portalNav?.items ?? []}
        theme={workspaceTheme}
        utilityLinks={portalNav?.utilityLinks}
        showRail={false}
        showActiveBadge={false}
      >
        <div className="setup-lane-shell">
          <OnboardingJourney
            roadmap={portalNav?.roadmap}
            currentStep={onboardingCurrentStep}
            accountComplete
            planComplete
            ownerComplete={workspace.setupChecklist.ownerAccessConfigured}
            billingComplete={workspace.setupChecklist.ownerAccessConfigured}
            shopComplete={workspace.setupChecklist.shopDetailsComplete}
            programmeComplete={workspace.setupChecklist.hasAnyProgramme}
            accountUrl="/portal/signup"
            planUrl={`/portal/signup/plan/${merchantId}`}
            ownerUrl={`/portal/signup/billing/${merchantId}?plan=starter&stage=owner`}
            billingUrl={`/portal/signup/billing/${merchantId}?plan=starter&stage=billing`}
            shopUrl={shopUrl}
            programmeUrl={programmeUrl}
            variant="compact"
          />

          <section className="section-intro space-y-3">
            <h1 className="luxe-title">{setupLaneTitle}</h1>
            <p className="luxe-subtitle text-foreground/88">{setupLaneSummary}</p>
          </section>

          <Card id="setup-lane" className="setup-lane-card">
            <CardHeader>
              <div className="flex flex-wrap items-center justify-between gap-2">
                <CardTitle>{setupLaneActionKey === "shop" ? "Shop details" : "Programme template"}</CardTitle>
                <Badge className="border-[rgba(200,169,106,0.24)] bg-[rgba(200,169,106,0.12)] text-[#6f592f]">
                  Step {setupLaneActionKey === "shop" ? "5" : "6"} of 6
                </Badge>
              </div>
              <CardDescription>
                {setupLaneActionKey === "shop"
                  ? "Enter the operating details used for join pages and wallet previews."
                  : "Choose the first programme template to open Configure and Customers."}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex flex-wrap gap-2">
                {setupLaneTasks.map((task) => (
                  <span
                    key={task.key}
                    className={`inline-flex items-center rounded-full border px-3 py-1 text-xs ${
                      task.isComplete
                        ? "border-primary/35 bg-primary/10 text-foreground"
                        : task.isBlocked
                          ? "border-border/70 bg-background text-muted-foreground"
                          : "border-secondary/45 bg-secondary/10 text-foreground"
                    }`}
                  >
                    {task.label}: {task.isComplete ? "Complete" : task.isBlocked ? "Blocked" : "Required"}
                  </span>
                ))}
              </div>

              {setupLaneActionKey === "shop" ? (
                <div className="grid gap-4 sm:grid-cols-2">
                  <section className="setup-lane-section sm:col-span-2">
                    <div className="space-y-2">
                      <Label htmlFor="shop-type-inline">Shop type</Label>
                      <Select
                        value={shopTypeKey}
                        onValueChange={(value) => {
                          setShopTypeKey(value);
                          setIsShopDraftDirty(true);
                        }}
                      >
                        <SelectTrigger id="shop-type-inline" className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.96)] text-base">
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
                  </section>
                  <section className="setup-lane-section">
                    <div className="space-y-2">
                      <Label htmlFor="shop-town-inline">Town or city</Label>
                      <Input
                        id="shop-town-inline"
                        value={shopTownOrCity}
                        onChange={(event) => {
                          setShopTownOrCity(event.target.value);
                          setIsShopDraftDirty(true);
                        }}
                        placeholder="Ipswich"
                        className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.96)] text-base"
                      />
                    </div>
                  </section>
                  <section className="setup-lane-section">
                    <div className="space-y-2">
                      <Label htmlFor="shop-postcode-inline">Postcode</Label>
                      <Input
                        id="shop-postcode-inline"
                        value={shopPostcode}
                        onChange={(event) => {
                          setShopPostcode(event.target.value);
                          setIsShopDraftDirty(true);
                        }}
                        placeholder="IP4 2XP"
                        className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.96)] text-base"
                      />
                    </div>
                  </section>
                  <section className="setup-lane-section sm:col-span-2">
                    <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Brand logo</p>
                    <p className="mt-1 text-sm text-foreground/76">Upload a PNG logo for join pages and wallet previews.</p>
                    <form className="mt-3 flex flex-wrap items-center gap-3" onSubmit={handleUploadLogo}>
                      <Input
                        type="file"
                        accept="image/png"
                        onChange={(event) => setBrandLogoFile(event.target.files?.[0] ?? null)}
                        className="max-w-sm rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.96)]"
                      />
                      <Button type="submit" variant="outline" className="border-[rgba(15,27,42,0.14)] bg-transparent text-[#0f1b2a] hover:bg-[rgba(15,27,42,0.04)]" disabled={isUploadingLogo || !brandLogoFile}>
                        {isUploadingLogo ? "Uploading..." : "Upload logo"}
                      </Button>
                    </form>
                    {brandLogoUrl ? (
                      <div className="mt-4 rounded-[1.2rem] border border-[rgba(15,27,42,0.1)] bg-white/70 p-4">
                        <p className="mb-2 text-xs uppercase tracking-[0.14em] text-muted-foreground">Current logo</p>
                        <Image
                          src={brandLogoUrl}
                          alt={`${workspace.merchant.displayName} logo`}
                          width={Math.max(workspace.brandProfile.logoWidth || 96, 72)}
                          height={Math.max(workspace.brandProfile.logoHeight || 96, 72)}
                          className="h-auto max-h-16 w-auto rounded-md border border-border/70 bg-white/90 p-1"
                          unoptimized
                        />
                      </div>
                    ) : null}
                  </section>
                  <div className="sm:col-span-2">
                    <Button
                      type="button"
                      className="rounded-full bg-[#0f1b2a] text-[#f5f3ef] hover:bg-[#18283a]"
                      onClick={() => {
                        void submitShopDetails();
                      }}
                      disabled={isSavingShopDetails}
                    >
                      {isSavingShopDetails ? "Saving..." : "Save shop details"}
                    </Button>
                  </div>
                </div>
              ) : (
                <div className="setup-lane-section space-y-4">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <p className="text-sm text-foreground/76">Shop details are complete. Choose the first programme to publish.</p>
                    <Button
                      type="button"
                      variant="outline"
                      className="border-[rgba(15,27,42,0.14)] bg-transparent text-[#0f1b2a] hover:bg-[rgba(15,27,42,0.04)]"
                      onClick={() => setIsShopSetupOpen(true)}
                    >
                      Edit shop details
                    </Button>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="template">Template</Label>
                    <Select value={selectedTemplateKey} onValueChange={setSelectedTemplateKey}>
                      <SelectTrigger id="template" className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.96)] text-base">
                        <SelectValue placeholder="Choose template" />
                      </SelectTrigger>
                      <SelectContent>
                        {templates.map((template) => (
                          <SelectItem key={template.templateKey} value={template.templateKey}>
                            {template.templateLabel}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <Button className="w-full rounded-full bg-[#0f1b2a] text-[#f5f3ef] hover:bg-[#18283a] sm:w-auto" onClick={handleCreateProgramme} disabled={isMutating || !selectedTemplateKey}>
                    {isMutating ? "Creating..." : setupLaneCtaLabel}
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>

          <Dialog open={isShopSetupOpen} onOpenChange={setIsShopSetupOpen}>
            <DialogContent className="max-h-[88vh] overflow-y-auto border-[rgba(15,27,42,0.12)] bg-[rgba(255,251,245,0.98)] p-5 sm:max-w-2xl sm:p-6">
              <DialogHeader>
                <DialogTitle>Shop setup</DialogTitle>
                <DialogDescription>Update the shop details used for join pages and wallet previews.</DialogDescription>
              </DialogHeader>

              <form className="mt-2 grid gap-4 sm:grid-cols-2" onSubmit={handleSaveShopDetails}>
                <div className="space-y-2 sm:col-span-2">
                  <Label htmlFor="shop-type">Shop type</Label>
                  <Select
                    value={shopTypeKey}
                    onValueChange={(value) => {
                      setShopTypeKey(value);
                      setIsShopDraftDirty(true);
                    }}
                  >
                    <SelectTrigger id="shop-type" className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.96)] text-base">
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
                <div className="space-y-2">
                  <Label htmlFor="shop-town">Town or city</Label>
                  <Input
                    id="shop-town"
                    value={shopTownOrCity}
                    onChange={(event) => {
                      setShopTownOrCity(event.target.value);
                      setIsShopDraftDirty(true);
                    }}
                    placeholder="Bristol"
                    className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.96)] text-base"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="shop-postcode">Postcode</Label>
                  <Input
                    id="shop-postcode"
                    value={shopPostcode}
                    onChange={(event) => {
                      setShopPostcode(event.target.value);
                      setIsShopDraftDirty(true);
                    }}
                    placeholder="BS1 4DJ"
                    className="h-14 rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.96)] text-base"
                  />
                </div>
                <div className="sm:col-span-2">
                  <Button
                    type="submit"
                    className="rounded-full bg-[#0f1b2a] text-[#f5f3ef] hover:bg-[#18283a]"
                    disabled={isSavingShopDetails}
                  >
                    {isSavingShopDetails ? "Saving..." : "Save shop details"}
                  </Button>
                </div>
              </form>

              <div className="mt-4 rounded-[1.4rem] border border-[rgba(15,27,42,0.1)] bg-[rgba(255,252,247,0.92)] p-4">
                <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Brand logo</p>
                <p className="mt-1 text-sm text-foreground/75">Upload a PNG logo for join pages and wallet previews.</p>
                <form className="mt-3 flex flex-wrap items-center gap-3" onSubmit={handleUploadLogo}>
                  <Input
                    type="file"
                    accept="image/png"
                    onChange={(event) => setBrandLogoFile(event.target.files?.[0] ?? null)}
                    className="max-w-sm rounded-2xl border-[rgba(15,27,42,0.14)] bg-[rgba(255,252,247,0.96)]"
                  />
                  <Button type="submit" variant="outline" className="border-[rgba(15,27,42,0.14)] bg-transparent text-[#0f1b2a] hover:bg-[rgba(15,27,42,0.04)]" disabled={isUploadingLogo || !brandLogoFile}>
                    {isUploadingLogo ? "Uploading..." : "Upload logo"}
                  </Button>
                </form>
                {brandLogoUrl ? (
                  <div className="mt-3 rounded-[1.2rem] border border-[rgba(15,27,42,0.1)] bg-white/70 p-3">
                    <p className="mb-2 text-xs uppercase tracking-[0.14em] text-muted-foreground">Current logo preview</p>
                    <Image
                      src={brandLogoUrl}
                      alt={`${workspace.merchant.displayName} logo`}
                      width={Math.max(workspace.brandProfile.logoWidth || 96, 72)}
                      height={Math.max(workspace.brandProfile.logoHeight || 96, 72)}
                      className="h-auto max-h-16 w-auto rounded-md border border-border/70 bg-white/80 p-1"
                      unoptimized
                    />
                  </div>
                ) : null}
              </div>
            </DialogContent>
          </Dialog>

          {error ? <p className="text-sm text-destructive">{error}</p> : null}
          {message ? <p className="text-sm text-foreground/74">{message}</p> : null}
        </div>
      </PortalShell>
    );
  }

  return (
    <PortalShell
      title="Merchant portal"
      activeKey={portalNav?.activeKey ?? section}
      railItems={portalNav?.items ?? []}
      theme={workspaceTheme}
      utilityLinks={portalNav?.utilityLinks}
      showRail={false}
      showActiveBadge={false}
    >
      <div className="space-y-4">
        <section className="workspace-hero section-intro space-y-3">
          <div className="relative z-10 flex flex-wrap items-start justify-between gap-4">
            <div className="min-w-0 flex-1 space-y-2">
              <div className="flex items-center gap-2">
                <Badge>Merchant workspace</Badge>
                {brandLogoUrl ? (
                  <span className="brand-logo-plate">
                    <Image
                      src={brandLogoUrl}
                      alt={`${workspace.merchant.displayName} logo`}
                      width={Math.max(workspace.brandProfile.logoWidth || 56, 40)}
                      height={Math.max(workspace.brandProfile.logoHeight || 56, 40)}
                      className="h-8 w-auto"
                      unoptimized
                    />
                  </span>
                ) : null}
              </div>
              <h1 className="luxe-title">{workspace.merchant.displayName}</h1>
              <p className="max-w-3xl text-balance text-[1.08rem] leading-8 text-foreground/88 sm:text-[1.2rem]">
                Operate daily loyalty, refine programme settings, and manage customer cards with a mobile-first control lane.
              </p>
            </div>

            <nav className="w-full rounded-2xl border border-border/70 bg-background/70 p-1.5 md:w-auto" aria-label="Workspace sections">
              <div className="flex flex-wrap gap-1">
                {(portalNav?.items ?? []).map((item) => (
                  <Link
                    key={item.key}
                    href={item.href}
                    className={`rounded-full border px-3 py-1.5 text-sm transition ${
                      item.key === (portalNav?.activeKey ?? section)
                        ? "border-[var(--portal-primary)] bg-[color-mix(in_srgb,var(--portal-primary)_12%,white_88%)] text-foreground"
                        : "border-border/70 bg-background/70 text-foreground/78 hover:bg-[color-mix(in_srgb,var(--portal-primary)_8%,white_92%)]"
                    } ${item.isDisabled ? "pointer-events-none opacity-45" : ""}`}
                  >
                    {item.label}
                  </Link>
                ))}
              </div>
            </nav>
          </div>

          <div className="relative z-10 grid gap-2.5 sm:grid-cols-4">
            <div className="workspace-metric-tile">
              <p className="text-[11px] uppercase tracking-[0.16em] text-muted-foreground">Location</p>
              <p className="mt-1 text-sm font-semibold">{workspace.merchant.townOrCity || "Town pending"}</p>
            </div>
            <div className="workspace-metric-tile">
              <p className="text-[11px] uppercase tracking-[0.16em] text-muted-foreground">Programmes</p>
              <p className="mt-1 text-sm font-semibold">{workspace.programmes.length}</p>
            </div>
            <div className="workspace-metric-tile">
              <p className="text-[11px] uppercase tracking-[0.16em] text-muted-foreground">Active cards</p>
              <p className="mt-1 text-sm font-semibold">{workspace.shopInsights.activeCards}</p>
            </div>
            <div className="workspace-metric-tile">
              <p className="text-[11px] uppercase tracking-[0.16em] text-muted-foreground">Rewards unlocked</p>
              <p className="mt-1 text-sm font-semibold">{workspace.shopInsights.rewardsUnlocked}</p>
            </div>
          </div>
        </section>

        {section === "operate" ? (
          selectedProgramme ? (
            <Card>
              <CardHeader>
                <CardTitle>{selectedProgramme.templateLabel}</CardTitle>
                <CardDescription className="text-foreground/72">{selectedProgramme.rewardCopy}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex flex-wrap gap-2">
                  <span className="metric-chip">Join: {selectedProgramme.joinCode ? `/join/${selectedProgramme.joinCode}` : "Unavailable"}</span>
                  <span className="metric-chip">Cards: {workspace.selectedProgrammeCards.length}</span>
                </div>

                <div className="flex flex-wrap gap-3">
                  <Button onClick={handleDemoJoin} disabled={isMutating}>Create demo customer join</Button>
                  {selectedProgramme.joinCode ? (
                    <Button asChild variant="outline">
                      <Link href={`/join/${selectedProgramme.joinCode}`} target="_blank" rel="noreferrer">Open join link</Link>
                    </Button>
                  ) : null}
                </div>

                <form className="space-y-3" onSubmit={handleAwardVisit}>
                  <Label htmlFor="scan-code">Scan card code</Label>
                  <Input
                    id="scan-code"
                    value={scanCode}
                    onChange={(event) => setScanCode(event.target.value)}
                    placeholder="card-..."
                  />
                  <Button type="submit" disabled={isMutating || scanCode.trim().length === 0}>Award visit</Button>
                </form>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent className="p-6 text-sm text-muted-foreground">
                No active programme selected.
              </CardContent>
            </Card>
          )
        ) : null}

        {section === "configure" ? (
          selectedProgramme ? (
            <Card>
              <CardHeader>
                <CardTitle>Configure programme</CardTitle>
                <CardDescription className="text-foreground/72">Set reward behaviour, campaign dates, and customer-facing copy.</CardDescription>
              </CardHeader>
              <CardContent>
                <form className="space-y-4" onSubmit={handleUpdateProgramme}>
                  <div className="grid gap-3 lg:grid-cols-2">
                    <section className="rounded-2xl border border-border/70 bg-background/85 p-4">
                      <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Reward rules</p>
                      <div className="mt-3 grid gap-3">
                        <div className="space-y-2">
                          <Label htmlFor="reward-item">Reward item</Label>
                          <Input id="reward-item" value={rewardItemLabel} onChange={(event) => setRewardItemLabel(event.target.value)} />
                        </div>
                        <div className="space-y-2">
                          <Label htmlFor="reward-threshold">Reward threshold</Label>
                          <Input
                            id="reward-threshold"
                            type="number"
                            min={1}
                            value={rewardThreshold}
                            onChange={(event) => setRewardThreshold(event.target.value)}
                          />
                        </div>
                      </div>
                    </section>

                    <section className="rounded-2xl border border-border/70 bg-background/85 p-4">
                      <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Campaign dates</p>
                      <div className="mt-3 grid gap-3">
                        <div className="space-y-2">
                          <Label htmlFor="starts-on">Starts on</Label>
                          <Input id="starts-on" type="date" value={startsOn} onChange={(event) => setStartsOn(event.target.value)} />
                        </div>
                        <div className="space-y-2">
                          <Label htmlFor="ends-on">Ends on</Label>
                          <Input id="ends-on" type="date" value={endsOn} onChange={(event) => setEndsOn(event.target.value)} />
                        </div>
                      </div>
                    </section>
                  </div>

                  <section className="rounded-2xl border border-border/70 bg-background/85 p-4">
                    <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Customer copy</p>
                    <div className="mt-3 space-y-2">
                      <Label htmlFor="reward-copy">Reward copy</Label>
                      <Input id="reward-copy" value={rewardCopy} onChange={(event) => setRewardCopy(event.target.value)} />
                    </div>
                  </section>

                  <div className="flex items-center justify-between rounded-2xl border border-border/70 bg-background/85 p-3">
                    <p className="text-xs text-foreground/70">Changes apply to the currently selected programme.</p>
                    <Button type="submit" disabled={isMutating || !canUpdateProgramme}>
                      Save programme
                    </Button>
                  </div>
                </form>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent className="p-6 text-sm text-muted-foreground">
                Complete onboarding in Operate to unlock configuration.
              </CardContent>
            </Card>
          )
        ) : null}

        {section === "customers" ? (
          selectedProgramme ? (
            <Card>
              <CardHeader>
                <CardTitle>Customer cards</CardTitle>
                <CardDescription className="text-foreground/72">
                  Manage active cards at scale with live status, progress, and redeem actions.
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <section className="rounded-2xl border border-border/70 bg-background/80 p-3">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div className="flex flex-wrap items-center gap-2">
                      <Badge variant="outline">{workspace.selectedProgrammeCards.length} cards</Badge>
                      <Badge variant="outline">{redeemableCardsCount} ready to redeem</Badge>
                    </div>
                    <div className="flex flex-wrap items-center gap-2">
                      <Button size="sm" onClick={handleDemoJoin} disabled={isMutating}>
                        Create demo join
                      </Button>
                      {selectedProgramme.joinCode ? (
                        <Button asChild size="sm" variant="outline">
                          <Link href={`/join/${selectedProgramme.joinCode}`} target="_blank" rel="noreferrer">
                            Open join link
                          </Link>
                        </Button>
                      ) : null}
                    </div>
                  </div>
                  <div className="mt-3 grid gap-2 sm:grid-cols-[minmax(0,1fr)_13rem]">
                    <Input
                      value={cardFilter}
                      onChange={(event) => setCardFilter(event.target.value)}
                      placeholder="Filter by card code, status, reward, or progress"
                    />
                    <Select value={cardStatusFilter} onValueChange={(value) => setCardStatusFilter(value as CardStatusFilter)}>
                      <SelectTrigger aria-label="Card status filter">
                        <SelectValue placeholder="Filter status" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="all">All statuses</SelectItem>
                        <SelectItem value="ready">Reward ready</SelectItem>
                        <SelectItem value="active">Active</SelectItem>
                        <SelectItem value="redeemed">Redeemed</SelectItem>
                        <SelectItem value="scheduled">Scheduled</SelectItem>
                        <SelectItem value="expired">Expired</SelectItem>
                        <SelectItem value="suspended">Suspended</SelectItem>
                        <SelectItem value="archived">Archived</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="mt-3 flex flex-wrap items-center gap-2 rounded-xl border border-border/70 bg-card/70 p-2.5">
                    <Badge variant="outline">{selectedCardsCount} selected</Badge>
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      disabled={isMutating || selectedCardsCount === 0}
                      onClick={() => handleBulkLifecycleAction("suspend")}
                    >
                      Suspend selected
                    </Button>
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      disabled={isMutating || selectedCardsCount === 0}
                      onClick={() => handleBulkLifecycleAction("reactivate")}
                    >
                      Reactivate selected
                    </Button>
                    <Button
                      type="button"
                      size="sm"
                      variant="outline"
                      disabled={isMutating || selectedCardsCount === 0}
                      onClick={() => handleBulkLifecycleAction("archive")}
                    >
                      Archive selected
                    </Button>
                    {selectedCardsCount > 0 ? (
                      <Button type="button" size="sm" variant="ghost" onClick={() => setSelectedCardIds([])}>
                        Clear
                      </Button>
                    ) : null}
                  </div>
                  {workspace.selectedProgrammeCards.length > 0 ? (
                    <p className="mt-2 text-xs text-muted-foreground">
                      Showing {filteredCards.length} of {workspace.selectedProgrammeCards.length} cards.
                    </p>
                  ) : null}
                </section>

                {workspace.selectedProgrammeCards.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No cards yet. Create a demo join from Operate.</p>
                ) : (
                  <div className="overflow-x-auto rounded-2xl border border-border/70 bg-background/85">
                    <table className="min-w-[62rem] w-full text-sm">
                      <thead>
                        <tr className="border-b border-border/70 text-left text-xs uppercase tracking-[0.14em] text-muted-foreground">
                          <th className="px-3 py-2 font-medium">Select</th>
                          <th className="px-3 py-2 font-medium">Card</th>
                          <th className="px-3 py-2 font-medium">Status</th>
                          <th className="px-3 py-2 font-medium">Progress</th>
                          <th className="px-3 py-2 font-medium">Window</th>
                          <th className="px-3 py-2 font-medium">Updated</th>
                          <th className="px-3 py-2 font-medium text-right">Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {filteredCards.map((card) => (
                          <tr key={card.cardId} className="border-b border-border/55 align-top last:border-b-0">
                            <td className="px-3 py-3">
                              <input
                                type="checkbox"
                                aria-label={`Select card ${card.cardCode}`}
                                checked={selectedCardIds.includes(card.cardId)}
                                onChange={(event) => {
                                  setSelectedCardIds((current) =>
                                    event.target.checked
                                      ? (current.includes(card.cardId) ? current : [...current, card.cardId])
                                      : current.filter((item) => item !== card.cardId),
                                  );
                                }}
                              />
                            </td>
                            <td className="px-3 py-3">
                              <div className="flex items-center gap-3">
                                <div className="relative h-14 w-24 overflow-hidden rounded-lg border border-border/60 shadow-sm">
                                  <div
                                    className="absolute inset-0"
                                    style={{
                                      background: `linear-gradient(132deg, ${card.primaryColor}, ${card.accentColor})`,
                                    }}
                                  />
                                  <div className="relative flex h-full flex-col justify-between p-1.5 text-[10px] leading-tight text-white">
                                    <span className="truncate font-semibold">{workspace.merchant.displayName}</span>
                                    <span className="truncate text-white/90">{card.rewardItemLabel}</span>
                                    <span className="font-medium text-white/95">{card.currentCount}/{card.targetCount}</span>
                                  </div>
                                </div>
                                <div className="space-y-0.5">
                                  <p className="text-sm font-semibold">{card.cardCode}</p>
                                  <p className="text-xs text-muted-foreground">{card.rewardCopy}</p>
                                </div>
                              </div>
                            </td>
                            <td className="px-3 py-3">
                              <span className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-medium ${getCardStatusClass(card)}`}>
                                {card.customerCardStatusLabel}
                              </span>
                            </td>
                            <td className="px-3 py-3 text-foreground/84">{card.progressDisplayText}</td>
                            <td className="px-3 py-3 text-foreground/76">
                              {formatDateLabel(card.startsOn)} to {formatDateLabel(card.endsOn)}
                            </td>
                            <td className="px-3 py-3 text-foreground/76">{formatDateLabel(card.lastUpdatedUtc)}</td>
                            <td className="px-3 py-3">
                              <div className="flex items-center justify-end gap-2">
                                <Button
                                  type="button"
                                  size="sm"
                                  variant="outline"
                                  onClick={() => handleOpenCardDetail(card.cardId)}
                                >
                                  <Eye className="h-3.5 w-3.5" />
                                  Details
                                </Button>
                                <Button
                                  type="button"
                                  size="sm"
                                  variant="outline"
                                  onClick={() => handleCopyCardCode(card.cardCode)}
                                >
                                  <Copy className="h-3.5 w-3.5" />
                                  Copy code
                                </Button>
                                <Button
                                  type="button"
                                  size="sm"
                                  variant="secondary"
                                  disabled={isMutating || !card.canRedeem}
                                  onClick={() => handleRedeem(card.cardId)}
                                >
                                  Redeem
                                </Button>
                                <Button
                                  type="button"
                                  size="sm"
                                  variant="outline"
                                  disabled={isMutating}
                                  onClick={() => handleLifecycleAction(card.cardId, "suspend")}
                                >
                                  Suspend
                                </Button>
                                <Button
                                  type="button"
                                  size="sm"
                                  variant="outline"
                                  disabled={isMutating}
                                  onClick={() => handleLifecycleAction(card.cardId, "reactivate")}
                                >
                                  Reactivate
                                </Button>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
                {workspace.selectedProgrammeCards.length > 0 && filteredCards.length === 0 ? (
                  <p className="text-sm text-muted-foreground">No cards match that filter.</p>
                ) : null}

                <Dialog
                  open={isCardDetailOpen}
                  onOpenChange={(open) => {
                    setIsCardDetailOpen(open);
                    if (!open) {
                      setSelectedCardId(null);
                    }
                  }}
                >
                  <DialogContent className="max-h-[88vh] overflow-y-auto sm:max-w-2xl">
                    {selectedCard ? (
                      <div className="space-y-4">
                        <DialogHeader>
                          <DialogTitle>Card detail: {selectedCard.cardCode}</DialogTitle>
                          <DialogDescription>
                            Live status, progress, and recent activity for this customer card.
                          </DialogDescription>
                        </DialogHeader>

                        <section className="rounded-2xl border border-border/70 bg-background/85 p-4">
                          <div className="grid gap-3 sm:grid-cols-[9rem_minmax(0,1fr)]">
                            <div className="relative h-24 overflow-hidden rounded-lg border border-border/60 shadow-sm">
                              <div
                                className="absolute inset-0"
                                style={{
                                  background: `linear-gradient(132deg, ${selectedCard.primaryColor}, ${selectedCard.accentColor})`,
                                }}
                              />
                              <div className="relative flex h-full flex-col justify-between p-2 text-[11px] text-white">
                                <span className="truncate font-semibold">{workspace.merchant.displayName}</span>
                                <span className="truncate text-white/90">{selectedCard.rewardItemLabel}</span>
                                <span className="font-semibold">{selectedCard.currentCount}/{selectedCard.targetCount}</span>
                              </div>
                            </div>
                            <div className="grid gap-2 sm:grid-cols-2">
                              <div>
                                <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Status</p>
                                <span className={`mt-1 inline-flex rounded-full border px-2.5 py-1 text-xs font-medium ${getCardStatusClass(selectedCard)}`}>
                                  {selectedCard.customerCardStatusLabel}
                                </span>
                              </div>
                              <div>
                                <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Progress</p>
                                <p className="mt-1 text-sm font-medium">{selectedCard.progressDisplayText}</p>
                              </div>
                              <div>
                                <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Window</p>
                                <p className="mt-1 text-sm">{formatDateLabel(selectedCard.startsOn)} to {formatDateLabel(selectedCard.endsOn)}</p>
                              </div>
                              <div>
                                <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Updated</p>
                                <p className="mt-1 text-sm">{formatDateLabel(selectedCard.lastUpdatedUtc)}</p>
                              </div>
                            </div>
                          </div>
                        </section>

                        <section className="space-y-2 rounded-2xl border border-border/70 bg-background/85 p-4">
                          <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Actions</p>
                          <div className="flex flex-wrap gap-2">
                            <Button type="button" size="sm" variant="outline" onClick={() => handleCopyCardCode(selectedCard.cardCode)}>
                              <Copy className="h-3.5 w-3.5" />
                              Copy code
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              variant="secondary"
                              disabled={isMutating || !selectedCard.canRedeem}
                              onClick={() => handleRedeem(selectedCard.cardId)}
                            >
                              Redeem
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              variant="outline"
                              disabled={isMutating}
                              onClick={() => handleLifecycleAction(selectedCard.cardId, "suspend")}
                            >
                              Suspend
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              variant="outline"
                              disabled={isMutating}
                              onClick={() => handleLifecycleAction(selectedCard.cardId, "reactivate")}
                            >
                              Reactivate
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              variant="outline"
                              disabled={isMutating}
                              onClick={() => handleLifecycleAction(selectedCard.cardId, "archive")}
                            >
                              Archive
                            </Button>
                            <Button asChild type="button" size="sm" variant="outline">
                              <a href={`${apiBaseUrl}/api/v1/wallet/cards/${selectedCard.cardId}`} target="_blank" rel="noreferrer">
                                Card JSON
                                <ExternalLink className="h-3.5 w-3.5" />
                              </a>
                            </Button>
                          </div>
                        </section>

                        <section className="space-y-2 rounded-2xl border border-border/70 bg-background/85 p-4">
                          <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Recent activity</p>
                          {selectedCardTimeline.length === 0 ? (
                            <p className="text-sm text-muted-foreground">No recent card-specific timeline events.</p>
                          ) : (
                            <div className="space-y-2">
                              {selectedCardTimeline.map((event) => (
                                <article key={event.eventId} className="rounded-xl border border-border/70 bg-card/80 p-2.5">
                                  <p className="text-xs text-muted-foreground">{formatDateLabel(event.occurredAtUtc)}</p>
                                  <p className="text-sm text-foreground/88">{event.summary}</p>
                                </article>
                              ))}
                            </div>
                          )}
                        </section>
                      </div>
                    ) : (
                      <div className="space-y-2 py-4">
                        <DialogTitle>Card detail unavailable</DialogTitle>
                        <DialogDescription>Select a card row to view details.</DialogDescription>
                      </div>
                    )}
                  </DialogContent>
                </Dialog>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent className="p-6 text-sm text-muted-foreground">
                Complete onboarding in Operate to view and redeem customer cards.
              </CardContent>
            </Card>
          )
        ) : null}

        {error ? <p className="text-sm text-destructive">{error}</p> : null}
        {message ? <p className="text-sm text-primary">{message}</p> : null}
      </div>
    </PortalShell>
  );
}
