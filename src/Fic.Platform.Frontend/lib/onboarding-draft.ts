"use client";

export type SignupMerchantDraft = {
  merchantId: string;
  displayName: string;
  contactEmail: string;
  ownerName?: string;
  shopTypeKey: string;
  townOrCity?: string;
  postcode?: string;
};

function keyFor(merchantId: string) {
  return `fic.signup.draft.${merchantId}`;
}

export function saveSignupMerchantDraft(draft: SignupMerchantDraft) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(keyFor(draft.merchantId), JSON.stringify(draft));
}

export function readSignupMerchantDraft(merchantId: string): SignupMerchantDraft | null {
  if (typeof window === "undefined") {
    return null;
  }

  try {
    const raw = window.localStorage.getItem(keyFor(merchantId));
    if (!raw) {
      return null;
    }

    const parsed = JSON.parse(raw) as SignupMerchantDraft;
    if (!parsed?.merchantId) {
      return null;
    }

    return parsed;
  } catch {
    return null;
  }
}
