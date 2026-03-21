export type ShopTypeOption = {
  shopTypeKey: string;
  shopTypeLabel: string;
  description: string;
  isActive: boolean;
};

export type ProgrammeTemplateOption = {
  templateKey: string;
  templateLabel: string;
  programmeTypeKey: string;
  programmeTypeLabel: string;
  headline: string;
  description: string;
  rewardItemLabel: string;
  rewardThreshold: number;
  rewardCopy: string;
  deliveryTypeKey: string;
  deliveryTypeLabel: string;
  outputLabel: string;
  shopTypeKey: string;
  cardTypeKey: string;
  cardTypeLabel: string;
  isActive: boolean;
};

export type LoyaltyProgrammeSnapshot = {
  programmeId: string;
  templateKey: string;
  templateLabel: string;
  programmeTypeKey: string;
  programmeTypeLabel: string;
  deliveryTypeKey: string;
  deliveryTypeLabel: string;
  cardTypeKey: string;
  cardTypeLabel: string;
  outputLabel: string;
  rewardItemLabel: string;
  rewardThreshold: number;
  rewardCopy: string;
  startsOn: string;
  endsOn: string;
  joinCode: string;
};

export type ProgrammeSummarySnapshot = {
  programmeId: string;
  templateKey: string;
  templateLabel: string;
  programmeTypeKey: string;
  programmeTypeLabel: string;
  deliveryTypeKey: string;
  deliveryTypeLabel: string;
  cardTypeKey: string;
  cardTypeLabel: string;
  outputLabel: string;
  rewardHeadline: string;
  rewardItemLabel: string;
  rewardThreshold: number;
  startsOn: string;
  endsOn: string;
  joinCode: string;
  activeCards: number;
  rewardsUnlocked: number;
};

export type MerchantSnapshot = {
  merchantId: string;
  displayName: string;
  townOrCity: string;
  postcode: string;
  contactEmail: string;
  shopTypeKey: string;
};

export type SetupChecklistSnapshot = {
  shopDetailsComplete: boolean;
  brandComplete: boolean;
  hasAnyProgramme: boolean;
  ownerAccessConfigured: boolean;
  joinReady: boolean;
};

export type MerchantWorkspaceSnapshot = {
  merchant: MerchantSnapshot;
  brandProfile: {
    logoUrl: string;
    primaryColor: string;
    accentColor: string;
    logoWidth: number;
    logoHeight: number;
  };
  setupChecklist: SetupChecklistSnapshot;
  shopInsights: {
    programmesCount: number;
    activeCards: number;
    rewardsUnlocked: number;
  };
  programmes: ProgrammeSummarySnapshot[];
  selectedProgramme?: LoyaltyProgrammeSnapshot | null;
  selectedJoinUrl?: string | null;
  selectedProgrammeCards: WalletCardSnapshot[];
  timeline: TimelineEventSnapshot[];
};

export type SessionSummaryResponse = {
  isAuthenticated: boolean;
  merchant?: {
    merchantId: string;
    contactEmail: string;
    displayName: string;
  } | null;
};

export type WalletCardSnapshot = {
  cardId: string;
  merchantId: string;
  programmeId: string;
  cardCode: string;
  walletPassId: string;
  vendorDisplayName: string;
  logoUrl: string;
  rewardItemLabel: string;
  rewardCopy: string;
  startsOn: string;
  endsOn: string;
  primaryColor: string;
  accentColor: string;
  logoWidth: number;
  logoHeight: number;
  currentCount: number;
  targetCount: number;
  progressDisplayText: string;
  rewardState: number;
  customerCardStatus: number;
  customerCardStatusLabel: string;
  canRedeem: boolean;
  lastUpdatedUtc: string;
};

export type TimelineEventSnapshot = {
  eventId: string;
  eventType: string;
  summary: string;
  occurredAtUtc: string;
};

export type JoinExperienceSnapshot = {
  merchant: MerchantSnapshot;
  brandProfile: {
    logoUrl: string;
    primaryColor: string;
    accentColor: string;
    logoWidth: number;
    logoHeight: number;
  };
  programme: LoyaltyProgrammeSnapshot;
};
