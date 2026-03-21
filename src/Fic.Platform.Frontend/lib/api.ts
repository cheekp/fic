import type {
  JoinExperienceSnapshot,
  MerchantWorkspaceSnapshot,
  ProgrammeTemplateOption,
  SessionSummaryResponse,
  ShopTypeOption,
  WalletCardSnapshot,
} from "../types/contracts";
import type { PortalNavigationContract } from "../types/portal-contracts";

const apiBaseUrl = process.env.NEXT_PUBLIC_FIC_API_BASE_URL ?? "http://localhost:5276";

function resolveAssetUrl(pathOrUrl: string): string {
  if (!pathOrUrl) {
    return pathOrUrl;
  }

  if (/^https?:\/\//i.test(pathOrUrl) || pathOrUrl.startsWith("data:")) {
    return pathOrUrl;
  }

  if (pathOrUrl.startsWith("/")) {
    return `${apiBaseUrl}${pathOrUrl}`;
  }

  return pathOrUrl;
}

function normalizeWalletCardSnapshot(card: WalletCardSnapshot): WalletCardSnapshot {
  return {
    ...card,
    logoUrl: resolveAssetUrl(card.logoUrl),
  };
}

function normalizeWorkspaceSnapshot(snapshot: MerchantWorkspaceSnapshot): MerchantWorkspaceSnapshot {
  return {
    ...snapshot,
    brandProfile: {
      ...snapshot.brandProfile,
      logoUrl: resolveAssetUrl(snapshot.brandProfile.logoUrl),
    },
    selectedProgrammeCards: snapshot.selectedProgrammeCards.map(normalizeWalletCardSnapshot),
  };
}

function normalizeJoinExperienceSnapshot(snapshot: JoinExperienceSnapshot): JoinExperienceSnapshot {
  return {
    ...snapshot,
    brandProfile: {
      ...snapshot.brandProfile,
      logoUrl: resolveAssetUrl(snapshot.brandProfile.logoUrl),
    },
  };
}

async function readJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text();
    throw new Error(`API request failed (${response.status}): ${body}`);
  }

  return response.json() as Promise<T>;
}

export async function getShopTypes(): Promise<ShopTypeOption[]> {
  const response = await fetch(`${apiBaseUrl}/api/v1/catalogue/shop-types`, {
    method: "GET",
    cache: "no-store",
  });

  return readJson<ShopTypeOption[]>(response);
}

export async function createMerchant(input: {
  displayName: string;
  contactEmail: string;
  ownerName?: string;
  shopTypeKey?: string;
}) {
  const response = await fetch(`${apiBaseUrl}/api/v1/merchants`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(input),
    credentials: "include",
  });

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function uploadMerchantLogo(merchantId: string, logoFile: File, selectedProgrammeId?: string) {
  const formData = new FormData();
  formData.append("logo", logoFile);

  const url = new URL(`${apiBaseUrl}/api/v1/merchants/${merchantId}/brand/logo`);
  if (selectedProgrammeId) {
    url.searchParams.set("selectedProgrammeId", selectedProgrammeId);
  }

  const response = await fetch(url, {
    method: "POST",
    body: formData,
    credentials: "include",
  });

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function getSession(): Promise<SessionSummaryResponse> {
  const response = await fetch(`${apiBaseUrl}/api/v1/session/current`, {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });

  return readJson<SessionSummaryResponse>(response);
}

export async function getWorkspace(merchantId: string): Promise<MerchantWorkspaceSnapshot> {
  const response = await fetch(`${apiBaseUrl}/api/v1/merchants/${merchantId}/workspace`, {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function getWorkspaceForProgramme(
  merchantId: string,
  programmeId?: string,
): Promise<MerchantWorkspaceSnapshot> {
  const url = new URL(`${apiBaseUrl}/api/v1/merchants/${merchantId}/workspace`);
  if (programmeId) {
    url.searchParams.set("programmeId", programmeId);
  }

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function getSignupPortalNavigation(
  step: "signup" | "plan" | "owner" | "billing",
  merchantId?: string,
): Promise<PortalNavigationContract> {
  const url = new URL(`${apiBaseUrl}/api/v1/portal/navigation/signup`);
  url.searchParams.set("step", step);
  if (merchantId) {
    url.searchParams.set("merchantId", merchantId);
  }

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });

  return readJson<PortalNavigationContract>(response);
}

export async function getWorkspacePortalNavigation(
  merchantId: string,
  step: "operate" | "configure" | "customers",
  programmeId?: string,
): Promise<PortalNavigationContract> {
  const url = new URL(`${apiBaseUrl}/api/v1/merchants/${merchantId}/portal/navigation`);
  url.searchParams.set("step", step);
  if (programmeId) {
    url.searchParams.set("programmeId", programmeId);
  }

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });

  return readJson<PortalNavigationContract>(response);
}

export async function getProgrammeTemplates(shopTypeKey?: string): Promise<ProgrammeTemplateOption[]> {
  const url = new URL(`${apiBaseUrl}/api/v1/catalogue/templates`);
  if (shopTypeKey) {
    url.searchParams.set("shopTypeKey", shopTypeKey);
  }

  const response = await fetch(url, {
    method: "GET",
    cache: "no-store",
  });

  return readJson<ProgrammeTemplateOption[]>(response);
}

export async function createProgramme(merchantId: string, templateKey: string): Promise<MerchantWorkspaceSnapshot> {
  const response = await fetch(`${apiBaseUrl}/api/v1/merchants/${merchantId}/programmes`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ templateKey }),
    credentials: "include",
  });

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function updateMerchantBrand(
  merchantId: string,
  payload: {
    displayName: string;
    townOrCity: string;
    postcode: string;
    contactEmail: string;
    shopTypeKey: string;
    primaryColor: string;
    accentColor: string;
    selectedProgrammeId?: string;
  },
): Promise<MerchantWorkspaceSnapshot> {
  const response = await fetch(`${apiBaseUrl}/api/v1/merchants/${merchantId}/brand`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
    credentials: "include",
  });

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function updateProgramme(
  merchantId: string,
  programmeId: string,
  payload: {
    rewardItemLabel: string;
    rewardThreshold: number;
    rewardCopy: string;
    startsOn: string;
    endsOn: string;
  },
): Promise<MerchantWorkspaceSnapshot> {
  const response = await fetch(`${apiBaseUrl}/api/v1/merchants/${merchantId}/programmes/${programmeId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
    credentials: "include",
  });

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function awardVisit(
  merchantId: string,
  programmeId: string,
  scannedCode: string,
): Promise<MerchantWorkspaceSnapshot> {
  const response = await fetch(
    `${apiBaseUrl}/api/v1/merchants/${merchantId}/programmes/${programmeId}/award-visit`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ scannedCode }),
      credentials: "include",
    },
  );

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function redeemReward(
  merchantId: string,
  programmeId: string,
  cardId: string,
): Promise<MerchantWorkspaceSnapshot> {
  const response = await fetch(
    `${apiBaseUrl}/api/v1/merchants/${merchantId}/programmes/${programmeId}/cards/${cardId}/redeem`,
    {
      method: "POST",
      credentials: "include",
    },
  );

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function updateCardLifecycle(
  merchantId: string,
  programmeId: string,
  cardId: string,
  action: "suspend" | "reactivate" | "archive",
): Promise<MerchantWorkspaceSnapshot> {
  const response = await fetch(
    `${apiBaseUrl}/api/v1/merchants/${merchantId}/programmes/${programmeId}/cards/${cardId}/lifecycle`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ action }),
      credentials: "include",
    },
  );

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function updateCardsLifecycleBulk(
  merchantId: string,
  programmeId: string,
  cardIds: string[],
  action: "suspend" | "reactivate" | "archive",
): Promise<MerchantWorkspaceSnapshot> {
  const response = await fetch(
    `${apiBaseUrl}/api/v1/merchants/${merchantId}/programmes/${programmeId}/cards/lifecycle`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ action, cardIds }),
      credentials: "include",
    },
  );

  const snapshot = await readJson<MerchantWorkspaceSnapshot>(response);
  return normalizeWorkspaceSnapshot(snapshot);
}

export async function joinProgramme(joinCode: string): Promise<WalletCardSnapshot> {
  const response = await fetch(`${apiBaseUrl}/api/v1/join/${joinCode}`, {
    method: "POST",
    credentials: "include",
  });

  const snapshot = await readJson<WalletCardSnapshot>(response);
  return normalizeWalletCardSnapshot(snapshot);
}

export async function getJoinExperience(joinCode: string): Promise<JoinExperienceSnapshot> {
  const response = await fetch(`${apiBaseUrl}/api/v1/join/${joinCode}`, {
    method: "GET",
    cache: "no-store",
  });

  const snapshot = await readJson<JoinExperienceSnapshot>(response);
  return normalizeJoinExperienceSnapshot(snapshot);
}

export async function completeSignup(input: {
  merchantId: string;
  plan: "starter";
  password: string;
  confirmPassword: string;
}) {
  const response = await fetch(`${apiBaseUrl}/api/v1/session/complete-signup`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(input),
    credentials: "include",
  });

  return readJson<SessionSummaryResponse>(response);
}
