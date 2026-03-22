namespace Fic.Contracts;

public sealed record PortalThemeContract(
    string Primary,
    string Accent,
    string Surface,
    string SurfaceStrong,
    string Ink,
    string MutedInk,
    string Line,
    string CanvasStart,
    string CanvasEnd,
    string PrimaryButton,
    string PrimaryButtonInk,
    string AccentSoft,
    string AccentInk,
    string LogoPlate,
    string LogoPlateBorder,
    string StampFilled,
    string StampEmpty,
    string StampInk,
    string Glow,
    string Variant,
    bool UseDarkChrome,
    string Radius,
    string Shadow);

public sealed record PortalNavItemContract(
    string Key,
    string Label,
    string Href,
    string? Badge,
    bool IsComplete,
    bool IsDisabled);

public sealed record PortalUtilityLinkContract(
    string Key,
    string Label,
    string Href,
    bool IsExternal);

public sealed record PortalRoadmapStepContract(
    string Key,
    string Label,
    string? CompactLabel,
    string Href,
    bool IsComplete,
    bool IsCurrent,
    bool IsNavigable);

public sealed record PortalRoadmapContract(
    string CurrentKey,
    int CompleteCount,
    int TotalCount,
    string? Hint,
    IReadOnlyList<PortalRoadmapStepContract> Steps);

public sealed record PortalNextActionTaskContract(
    string Key,
    string Label,
    bool IsComplete,
    bool IsBlocked,
    string? BlockedReason);

public sealed record PortalNextActionContract(
    string Key,
    string Title,
    string Summary,
    string CtaLabel,
    string? CtaHref,
    string? BlockedReason,
    IReadOnlyList<PortalNextActionTaskContract> Tasks);

public sealed record PortalNavigationContract(
    string Surface,
    string ActiveKey,
    PortalThemeContract Theme,
    IReadOnlyList<PortalNavItemContract> Items,
    IReadOnlyList<PortalUtilityLinkContract> UtilityLinks,
    PortalRoadmapContract? Roadmap,
    PortalNextActionContract? NextAction);
