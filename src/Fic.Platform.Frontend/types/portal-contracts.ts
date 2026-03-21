export type PortalNavKey =
  | "signup"
  | "plan"
  | "billing"
  | "operate"
  | "configure"
  | "customers";

export type PortalNavItemContract = {
  key: PortalNavKey;
  label: string;
  href: string;
  badge?: string;
  isComplete?: boolean;
  isDisabled?: boolean;
};

export type PortalThemeContract = {
  primary: string;
  accent: string;
  surface: string;
  ink: string;
  radius: "soft" | "rounded";
  shadow: "soft" | "medium";
};

export type PortalRoadmapStepContract = {
  key: "account" | "plan" | "owner" | "billing" | "shop" | "programme";
  label: string;
  compactLabel?: string | null;
  href: string;
  isComplete: boolean;
  isCurrent: boolean;
  isNavigable: boolean;
};

export type PortalRoadmapContract = {
  currentKey: PortalRoadmapStepContract["key"];
  completeCount: number;
  totalCount: number;
  hint?: string | null;
  steps: PortalRoadmapStepContract[];
};

export type PortalNavigationContract = {
  surface: "signup" | "workspace";
  activeKey: PortalNavKey;
  theme: PortalThemeContract;
  items: PortalNavItemContract[];
  roadmap?: PortalRoadmapContract | null;
};

export const ficPortalTheme: PortalThemeContract = {
  primary: "#1f3731",
  accent: "#f4c15d",
  surface: "rgb(255 250 240 / 0.82)",
  ink: "#14211d",
  radius: "rounded",
  shadow: "soft",
};
