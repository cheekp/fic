"use client";

import { useQuery } from "@tanstack/react-query";
import {
  getSession,
  getShopTypes,
  getSignupPortalNavigation,
  getWorkspaceForProgramme,
  getWorkspacePortalNavigation,
} from "@/lib/api";

type SignupStep = "signup" | "plan" | "owner" | "billing";
type WorkspaceStep = "operate" | "configure" | "customers";

export const queryKeys = {
  session: () => ["session", "current"] as const,
  signupPortalNav: (step: SignupStep, merchantId?: string) =>
    ["portal-nav", "signup", step, merchantId ?? "none"] as const,
  workspacePortalNav: (merchantId: string, step: WorkspaceStep, programmeId?: string) =>
    ["portal-nav", "workspace", merchantId, step, programmeId ?? "none"] as const,
  workspace: (merchantId: string, programmeId?: string) =>
    ["workspace", merchantId, programmeId ?? "none"] as const,
  shopTypes: () => ["catalogue", "shop-types"] as const,
};

export function useSessionSummaryQuery() {
  return useQuery({
    queryKey: queryKeys.session(),
    queryFn: getSession,
  });
}

export function useSignupPortalNavigationQuery(step: SignupStep, merchantId?: string) {
  return useQuery({
    queryKey: queryKeys.signupPortalNav(step, merchantId),
    queryFn: () => getSignupPortalNavigation(step, merchantId),
    enabled: step.length > 0,
  });
}

export function useWorkspacePortalNavigationQuery(
  merchantId: string,
  step: WorkspaceStep,
  programmeId?: string,
) {
  return useQuery({
    queryKey: queryKeys.workspacePortalNav(merchantId, step, programmeId),
    queryFn: () => getWorkspacePortalNavigation(merchantId, step, programmeId),
    enabled: Boolean(merchantId),
  });
}

export function useWorkspaceSnapshotQuery(merchantId: string, programmeId?: string) {
  return useQuery({
    queryKey: queryKeys.workspace(merchantId, programmeId),
    queryFn: () => getWorkspaceForProgramme(merchantId, programmeId),
    enabled: Boolean(merchantId),
  });
}

export function useShopTypesQuery() {
  return useQuery({
    queryKey: queryKeys.shopTypes(),
    queryFn: getShopTypes,
    staleTime: 5 * 60_000,
  });
}
