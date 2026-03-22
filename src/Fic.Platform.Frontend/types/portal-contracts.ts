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

export type PortalUtilityLinkContract = {
  key: string;
  label: string;
  href: string;
  isExternal: boolean;
};

export type PortalThemeContract = {
  primary: string;
  accent: string;
  surface: string;
  surfaceStrong: string;
  ink: string;
  mutedInk: string;
  line: string;
  canvasStart: string;
  canvasEnd: string;
  primaryButton: string;
  primaryButtonInk: string;
  accentSoft: string;
  accentInk: string;
  logoPlate: string;
  logoPlateBorder: string;
  stampFilled: string;
  stampEmpty: string;
  stampInk: string;
  glow: string;
  variant: "ribbon" | "glow" | "bloom";
  useDarkChrome: boolean;
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

export type PortalNextActionTaskContract = {
  key: "shop" | "programme";
  label: string;
  isComplete: boolean;
  isBlocked: boolean;
  blockedReason?: string | null;
};

export type PortalNextActionContract = {
  key: "shop" | "programme";
  title: string;
  summary: string;
  ctaLabel: string;
  ctaHref?: string | null;
  blockedReason?: string | null;
  tasks: PortalNextActionTaskContract[];
};

export type PortalNavigationContract = {
  surface: "signup" | "workspace";
  activeKey: PortalNavKey;
  theme: PortalThemeContract;
  items: PortalNavItemContract[];
  utilityLinks?: PortalUtilityLinkContract[];
  roadmap?: PortalRoadmapContract | null;
  nextAction?: PortalNextActionContract | null;
};

export const ficPortalTheme: PortalThemeContract = {
  primary: "#1f3731",
  accent: "#f4c15d",
  surface: "rgb(255 250 240 / 0.82)",
  surfaceStrong: "rgb(255 252 247 / 0.96)",
  ink: "#14211d",
  mutedInk: "rgba(20, 33, 29, 0.74)",
  line: "rgba(20, 33, 29, 0.1)",
  canvasStart: "#f3ecdd",
  canvasEnd: "#ebe0cf",
  primaryButton: "#1f3731",
  primaryButtonInk: "#f8f4ea",
  accentSoft: "rgba(244, 193, 93, 0.18)",
  accentInk: "#14211d",
  logoPlate: "rgba(250, 247, 241, 0.96)",
  logoPlateBorder: "rgba(31, 55, 49, 0.12)",
  stampFilled: "#f4c15d",
  stampEmpty: "rgba(31, 55, 49, 0.08)",
  stampInk: "#14211d",
  glow: "rgba(244, 193, 93, 0.16)",
  variant: "bloom",
  useDarkChrome: false,
  radius: "rounded",
  shadow: "soft",
};

export const northStarPortalTheme: PortalThemeContract = {
  primary: "#0f1b2a",
  accent: "#c8a96a",
  surface: "rgba(245, 243, 239, 0.9)",
  surfaceStrong: "rgba(255, 250, 244, 0.97)",
  ink: "#0f1b2a",
  mutedInk: "rgba(74, 79, 85, 0.82)",
  line: "rgba(15, 27, 42, 0.14)",
  canvasStart: "#0f1b2a",
  canvasEnd: "#1b2d40",
  primaryButton: "#0f1b2a",
  primaryButtonInk: "#f5f3ef",
  accentSoft: "rgba(200, 169, 106, 0.18)",
  accentInk: "#0f1b2a",
  logoPlate: "rgba(245, 243, 239, 0.96)",
  logoPlateBorder: "rgba(200, 169, 106, 0.34)",
  stampFilled: "#c8a96a",
  stampEmpty: "rgba(15, 27, 42, 0.08)",
  stampInk: "#0f1b2a",
  glow: "rgba(200, 169, 106, 0.18)",
  variant: "ribbon",
  useDarkChrome: true,
  radius: "rounded",
  shadow: "soft",
};
